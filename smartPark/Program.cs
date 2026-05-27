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
    public static async Task Main(string[] args)
    {
        var cultureInfo = new System.Globalization.CultureInfo("bs-BA");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        var builder = WebApplication.CreateBuilder(args);

        // Baza (Azure)
        var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        // ASP.NET Core Identity
        builder
            .Services.AddIdentity<Korisnik, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<smartPark.Helpers.BosnianIdentityErrorDescriber>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/Home/AccessDenied";
        });

        // MVC pattern
        builder.Services.AddControllersWithViews();

        // Session storage (cookie)
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Repozitoriji
        builder.Services.AddScoped<ICjenovnikRepository, CjenovnikRepository>();
        builder.Services.AddScoped<IParkingRepository, ParkingRepository>();
        builder.Services.AddScoped<IKorisnikRepository, KorisnikRepository>();
        builder.Services.AddScoped<IParkingMjestoRepository, ParkingMjestoRepository>();
        builder.Services.AddScoped<IRezervacijaRepository, RezervacijaRepository>();
        builder.Services.AddScoped<IQRKodRepository, QRKodRepository>();
        builder.Services.AddScoped<IIzvjestajRepository, IzvjestajRepository>();

        // Servisi
        builder.Services.AddScoped<IParkingService, ParkingService>();
        builder.Services.AddScoped<IKorisnikService, KorisnikService>();
        builder.Services.AddScoped<ICjenovnikService, CjenovnikService>();
        builder.Services.AddScoped<IParkingMjestoService, ParkingMjestoService>();
        builder.Services.AddScoped<IRezervacijaService, RezervacijaService>();
        builder.Services.AddScoped<IQRKodService, QRKodService>();
        builder.Services.AddScoped<IIzvjestajService, IzvjestajService>();

        // Email i podsjetnik
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddHostedService<RezervacijaPodsjetnikService>();

        // Swagger (opcionalno za dalje testiranje)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Hardkodirane uloge
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Rezervacije_ParkingMjestoId' AND object_id = OBJECT_ID('Rezervacije'))
                    BEGIN
                        DROP INDEX IX_Rezervacije_ParkingMjestoId ON Rezervacije;
                    END
                    CREATE INDEX IX_Rezervacije_ParkingMjestoId ON Rezervacije(ParkingMjestoId);

                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Parkinzi_MenadzerID' AND object_id = OBJECT_ID('Parkinzi'))
                    BEGIN
                        DROP INDEX IX_Parkinzi_MenadzerID ON Parkinzi;
                    END
                    CREATE INDEX IX_Parkinzi_MenadzerID ON Parkinzi(MenadzerID);

                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_MenadzerOdgovorniParkingId' AND object_id = OBJECT_ID('AspNetUsers'))
                    BEGIN
                        DROP INDEX IX_AspNetUsers_MenadzerOdgovorniParkingId ON AspNetUsers;
                    END
                    CREATE INDEX IX_AspNetUsers_MenadzerOdgovorniParkingId ON AspNetUsers(MenadzerOdgovorniParkingId);

                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID('Rezervacije') 
                        AND name = 'PocetakObavijestPoslana'
                    )
                    BEGIN
                        ALTER TABLE Rezervacije ADD PocetakObavijestPoslana bit NOT NULL DEFAULT 0;
                    END
                ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska prilikom popravke indeksa: {ex.Message}");
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Korisnik>>();

            string[] uloge = { "Vozac", "Menadzer", "Administrator" };

            foreach (var uloga in uloge)
            {
                if (!await roleManager.RoleExistsAsync(uloga))
                {
                    await roleManager.CreateAsync(new IdentityRole(uloga));
                    Console.WriteLine($"Kreirana uloga: {uloga}");
                }
            }

            var adminEmail = "admin@smartpark.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new Korisnik
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Ime = "Sistem",
                    Prezime = "Administrator",
                    Aktivan = true,
                    DatumRegistracije = DateTime.Now,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
            }

            // Automatsko kreiranje nedostajućih parking mjesta za postojeće parkinge
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var sviParkinzi = await dbContext.Parkinzi.Include(p => p.ParkingMjesta).ToListAsync();
            foreach (var p in sviParkinzi)
            {
                var trenutniBrojMjesta = p.ParkingMjesta.Count;
                if (trenutniBrojMjesta < p.UkupnoMjesta)
                {
                    for (int i = 1; i <= p.UkupnoMjesta; i++)
                    {
                        if (!p.ParkingMjesta.Any(m => m.BrojMjesta == i))
                        {
                            dbContext.ParkingMjesta.Add(new ParkingMjesto
                            {
                                ParkingId = p.ParkingId,
                                BrojMjesta = i,
                                StatusMjesta = smartPark.Models.Enums.StatusMjesta.Slobodno
                            });
                        }
                    }
                }
            }
            await dbContext.SaveChangesAsync();
        }

        // Middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // HTTP greške 404
        app.UseStatusCodePagesWithReExecute("/Home/NotFound");

        app.UseRouting();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();


        // Swagger middleware
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        await app.RunAsync();
    }
}
