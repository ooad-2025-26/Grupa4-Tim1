using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Enums;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations;

public class RezervacijaPodsjetnikService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RezervacijaPodsjetnikService> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public RezervacijaPodsjetnikService(
        IServiceScopeFactory scopeFactory,
        ILogger<RezervacijaPodsjetnikService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RezervacijaPodsjetnikService pokrenut.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PosaljiPodsjetnikeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u RezervacijaPodsjetnikService petlji.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task PosaljiPodsjetnikeAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var sada = DateTime.Now;

        // 1. Očisti istekle rezervacije i oslobodi mjesta
        await OcistiIstekleRezervacijeAsync(db, emailService, sada, ct);

        // 2. Aktiviraj započete rezervacije i pošalji email o početku
        await AktivirajZapočeteRezervacijeAsync(db, emailService, sada, ct);

        // 3. Pošalji podsjetnik početka (30 min prije)
        var pocetakOd = sada.AddMinutes(29);
        var pocetakDo = sada.AddMinutes(31);

        var rezervacijePocetak = await db.Rezervacije
            .Include(r => r.Korisnik)
            .Include(r => r.Parking)
            .Where(r =>
                r.StatusRezervacije == StatusRezervacije.Aktivna &&
                r.PocetakRezervacije >= pocetakOd &&
                r.PocetakRezervacije <= pocetakDo &&
                !r.PocetakPodsjetnikPoslan)
            .ToListAsync(ct);

        foreach (var r in rezervacijePocetak)
        {
            if (r.Korisnik?.Email == null) continue;

            try
            {
                await emailService.PosaljiPodsjetnikPocetkaAsync(
                    r.Korisnik.Email,
                    $"{r.Korisnik.Ime} {r.Korisnik.Prezime}",
                    r.RezervacijaId,
                    r.Parking?.Naziv ?? "Parking",
                    r.PocetakRezervacije,
                    r.Parking?.Adresa
                );

                r.PocetakPodsjetnikPoslan = true;
                _logger.LogInformation("Podsjetnik početka poslan za rezervaciju #{Id}", r.RezervacijaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Neuspješno slanje podsjetnika početka za rezervaciju #{Id}", r.RezervacijaId);
            }
        }

        // 4. Pošalji podsjetnik isteka (30 min prije isteka)
        var rezervacijeIstek = await db.Rezervacije
            .Include(r => r.Korisnik)
            .Include(r => r.Parking)
            .Where(r =>
                r.StatusRezervacije == StatusRezervacije.Aktivna &&
                r.KrajRezervacije >= pocetakOd &&
                r.KrajRezervacije <= pocetakDo &&
                !r.IstekPodsjetnikPoslan)
            .ToListAsync(ct);

        foreach (var r in rezervacijeIstek)
        {
            if (r.Korisnik?.Email == null) continue;

            try
            {
                await emailService.PosaljiPodsjetnikIstekaAsync(
                    r.Korisnik.Email,
                    $"{r.Korisnik.Ime} {r.Korisnik.Prezime}",
                    r.RezervacijaId,
                    r.Parking?.Naziv ?? "Parking",
                    r.KrajRezervacije
                );

                r.IstekPodsjetnikPoslan = true;
                _logger.LogInformation("Podsjetnik isteka poslan za rezervaciju #{Id}", r.RezervacijaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Neuspješno slanje podsjetnika isteka za rezervaciju #{Id}", r.RezervacijaId);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task AktivirajZapočeteRezervacijeAsync(ApplicationDbContext db, IEmailService emailService, DateTime sada, CancellationToken ct)
    {
        try
        {
            var zapoceteRezervacije = await db.Rezervacije
                .Include(r => r.Parking)
                .Include(r => r.Korisnik)
                .Where(r => r.StatusRezervacije == StatusRezervacije.Aktivna 
                            && r.PocetakRezervacije <= sada 
                            && r.KrajRezervacije >= sada
                            && (!r.PocetakObavijestPoslana || (r.ParkingMjestoId.HasValue && db.ParkingMjesta.Any(pm => pm.ParkingMjestoId == r.ParkingMjestoId.Value && pm.StatusMjesta == StatusMjesta.Slobodno))))
                .ToListAsync(ct);

            foreach (var r in zapoceteRezervacije)
            {
                // Ažuriraj status parking mjesta na Zauzeto
                if (r.ParkingMjestoId.HasValue)
                {
                    var pm = await db.ParkingMjesta.FindAsync(new object[] { r.ParkingMjestoId.Value }, ct);
                    if (pm != null && pm.StatusMjesta != StatusMjesta.Zauzeto)
                    {
                        pm.StatusMjesta = StatusMjesta.Zauzeto;
                        _logger.LogInformation("Rezervacija #{Id} je započela. Označavanje parking mjesta #{MjestoId} u 'Zauzeto'.", r.RezervacijaId, r.ParkingMjestoId.Value);
                    }
                }

                // Pošalji email obavijest o početku rezervacije
                if (!r.PocetakObavijestPoslana)
                {
                    if (r.Korisnik?.Email != null)
                    {
                        try
                        {
                            await emailService.PosaljiObavijestPocetkaRezervacijeAsync(
                                r.Korisnik.Email,
                                $"{r.Korisnik.Ime} {r.Korisnik.Prezime}",
                                r.RezervacijaId,
                                r.Parking?.Naziv ?? "Parking",
                                r.PocetakRezervacije,
                                r.KrajRezervacije,
                                r.Parking?.Adresa
                            );
                            _logger.LogInformation("Obavještenje o početku rezervacije poslano za rezervaciju #{Id}", r.RezervacijaId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Neuspješno slanje obavještenja o početku za rezervaciju #{Id}", r.RezervacijaId);
                        }
                    }
                    r.PocetakObavijestPoslana = true;
                }
            }

            if (zapoceteRezervacije.Count > 0)
            {
                await db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška prilikom aktivacije započetih rezervacija u pozadinskom servisu.");
        }
    }

    private async Task OcistiIstekleRezervacijeAsync(ApplicationDbContext db, IEmailService emailService, DateTime sada, CancellationToken ct)
    {
        try
        {
            var istekleRezervacije = await db.Rezervacije
                .Include(r => r.Parking)
                .Include(r => r.Korisnik)
                .Where(r => r.StatusRezervacije == StatusRezervacije.Aktivna && r.KrajRezervacije < sada)
                .ToListAsync(ct);

            foreach (var r in istekleRezervacije)
            {
                r.StatusRezervacije = StatusRezervacije.Istekla;
                _logger.LogInformation("Rezervacija #{Id} je istekla. Označavanje statusa u 'Istekla'.", r.RezervacijaId);

                // Oslobodi pripadajuće parking mjesto
                if (r.ParkingMjestoId.HasValue)
                {
                    var pm = await db.ParkingMjesta.FindAsync(new object[] { r.ParkingMjestoId.Value }, ct);
                    if (pm != null)
                    {
                        pm.StatusMjesta = StatusMjesta.Slobodno;
                    }
                }

                // Ažuriraj broj slobodnih mjesta na parkingu
                if (r.Parking != null)
                {
                    r.Parking.SlobodnaMjesta = Math.Min(r.Parking.UkupnoMjesta, r.Parking.SlobodnaMjesta + 1);
                }

                // Pošalji email obavještenje o završetku/isteku rezervacije
                if (r.Korisnik?.Email != null)
                {
                    try
                    {
                        await emailService.PosaljiObavijestPrekidaRezervacijeAsync(
                            r.Korisnik.Email,
                            $"{r.Korisnik.Ime} {r.Korisnik.Prezime}",
                            r.RezervacijaId,
                            r.Parking?.Naziv ?? "Parking",
                            "istekla"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Neuspješno slanje obavijesti isteka za rezervaciju #{Id}", r.RezervacijaId);
                    }
                }
            }

            if (istekleRezervacije.Count > 0)
            {
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("Uspješno ažurirano {Count} isteklih rezervacija i oslobođena parking mjesta.", istekleRezervacije.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška prilikom čišćenja isteklih rezervacija u pozadinskom servisu.");
        }
    }
}
