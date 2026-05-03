using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using smartPark.Models;

namespace smartPark.Data
{
    public class ApplicationDbContext : IdentityDbContext<Korisnik>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Parking> Parkinzi { get; set; }
        public DbSet<ParkingMjesto> ParkingMjesta { get; set; }
        public DbSet<Rezervacija> Rezervacije { get; set; }
        public DbSet<Notifikacija> Notifikacije { get; set; }
        public DbSet<Cjenovnik> Cjenovnici { get; set; }
        public DbSet<Izvjestaj> Izvjestaji { get; set; }
        public DbSet<QRKod> QRKodovi { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Parking>().Property(p => p.CijenaPoSatu).HasPrecision(10, 2);

            builder.Entity<Cjenovnik>().Property(c => c.CijenaPoSatu).HasPrecision(10, 2);

            builder.Entity<Rezervacija>().Property(r => r.UkupnaCijena).HasPrecision(10, 2);

            builder.Entity<Izvjestaj>().Property(i => i.UkupniPrihod).HasPrecision(18, 2);

            builder
                .Entity<Korisnik>()
                .HasIndex(k => k.BrojVozacke)
                .IsUnique()
                .HasFilter("[BrojVozacke] IS NOT NULL");

            builder
                .Entity<Korisnik>()
                .HasIndex(k => k.MenadzerOdgovorniParkingId)
                .IsUnique()
                .HasFilter("[MenadzerOdgovorniParkingId] IS NOT NULL");

            builder
                .Entity<Parking>()
                .HasOne(p => p.Menadzer)
                .WithOne(k => k.Parking)
                .HasForeignKey<Parking>(p => p.MenadzerID)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .Entity<ParkingMjesto>()
                .HasOne(pm => pm.Parking)
                .WithMany(p => p.ParkingMjesta)
                .HasForeignKey(pm => pm.ParkingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Rezervacija>()
                .HasOne(r => r.Korisnik)
                .WithMany(k => k.Rezervacije)
                .HasForeignKey(r => r.KorisnikId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Rezervacija>()
                .HasOne(r => r.Parking)
                .WithMany(p => p.Rezervacije)
                .HasForeignKey(r => r.ParkingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<Rezervacija>()
                .HasOne(r => r.ParkingMjesto)
                .WithOne(pm => pm.TrenutnaRezervacija)
                .HasForeignKey<Rezervacija>(r => r.ParkingMjestoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .Entity<QRKod>()
                .HasOne(q => q.Rezervacija)
                .WithOne(r => r.QRKodRezervacije)
                .HasForeignKey<QRKod>(q => q.RezervacijaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Notifikacija>()
                .HasOne(n => n.Korisnik)
                .WithMany(k => k.Notifikacije)
                .HasForeignKey(n => n.KorisnikId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Cjenovnik>()
                .HasOne(c => c.Parking)
                .WithMany()
                .HasForeignKey(c => c.ParkingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Izvjestaj>()
                .HasOne(i => i.Parking)
                .WithMany()
                .HasForeignKey(i => i.ParkingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rezervacija>().HasIndex(r => new { r.ParkingId, r.StatusRezervacije });

            builder.Entity<Rezervacija>().HasIndex(r => r.KorisnikId);

            builder.Entity<Rezervacija>().HasIndex(r => r.PocetakRezervacije);

            builder.Entity<Rezervacija>().HasIndex(r => r.KrajRezervacije);

            builder.Entity<Notifikacija>().HasIndex(n => n.KorisnikId);

            builder.Entity<Notifikacija>().HasIndex(n => n.DatumSlanja);

            builder.Entity<ParkingMjesto>().HasIndex(pm => pm.ParkingId);

            builder.Entity<ParkingMjesto>().HasIndex(pm => pm.StatusMjesta);

            builder.Entity<Cjenovnik>().HasIndex(c => new { c.ParkingId, c.Aktivan });

            builder.Entity<Cjenovnik>().HasIndex(c => c.DatumPocetka);

            builder.Entity<Izvjestaj>().HasIndex(i => i.ParkingId);

            builder.Entity<Izvjestaj>().HasIndex(i => i.DatumGenerisanja);

            builder.Entity<QRKod>().HasIndex(q => q.RezervacijaId).IsUnique();
        }
    }
}
