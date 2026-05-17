using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Repositories.Implementations;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Implementations;
using smartPark.Services.Interfaces;

namespace smartPark;

public class Program
{
    public static async Task Main(string[] args) // ← DODAJ "async Task" umjesto "void"
    {
        var builder = WebApplication.CreateBuilder(args);

        // ========== 1. DB CONTEXT ==========
        var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        // ========== 2. IDENTITY (SAMO JEDNOM!) ==========
        builder
            .Services.AddIdentity<Korisnik, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false; // Promijeni na false za lakše testiranje
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // ========== 3. DODATNI SERVISI ==========
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddControllersWithViews();

        // ========== 4. REPOSITORIJI ==========
        builder.Services.AddScoped<ICjenovnikRepository, CjenovnikRepository>();
        builder.Services.AddScoped<IParkingRepository, ParkingRepository>();
        builder.Services.AddScoped<IKorisnikRepository, KorisnikRepository>();

        // ========== 5. SERVISI ==========
        builder.Services.AddScoped<IParkingService, ParkingService>();
        builder.Services.AddScoped<IKorisnikService, KorisnikService>();
        builder.Services.AddScoped<ICjenovnikService, CjenovnikService>();

        var app = builder.Build();

        // ========== 6. HARDKODIRANJE ULOGA (ASYNC) ==========
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] uloge = { "Vozac", "Menadzer", "Administrator" };

            foreach (var uloga in uloge)
            {
                // OVDJE JE POPRAVKA - koristi await umjesto .Result
                if (!await roleManager.RoleExistsAsync(uloga))
                {
                    await roleManager.CreateAsync(new IdentityRole(uloga));
                    Console.WriteLine($"Kreirana uloga: {uloga}");
                }
                else
                {
                    Console.WriteLine($"Uloga već postoji: {uloga}");
                }
            }
        }

        // ========== 7. MIDDLEWARE ==========
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication(); // ← DODAJ OVO!
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        await app.RunAsync(); // ← KORISTI RunAsync umjesto Run
    }
}
