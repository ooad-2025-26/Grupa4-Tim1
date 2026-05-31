using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels;
using smartPark.Models.ViewModels.Rezervacija;
using smartPark.Services.Interfaces;
using System.Text.Json;

namespace smartPark.Controllers;

[Authorize]
public class RezervacijaController : Controller
{
    private readonly IRezervacijaService _rezervacijaService;
    private readonly UserManager<Korisnik> _userManager;
    private readonly IParkingService _parkingService;

    public RezervacijaController(
        IRezervacijaService rezervacijaService,
        UserManager<Korisnik> userManager,
        IParkingService parkingService
    )
    {
        _rezervacijaService = rezervacijaService;
        _userManager = userManager;
        _parkingService = parkingService;
    }


    [HttpGet("rezervacije")]
    [Authorize(Roles = "Administrator,Menadzer")]
    public async Task<IActionResult> Index(
        int? parkingId,
        string? status,
        DateTime? datumOd,
        DateTime? datumDo
    )
    {
        var viewModel = await _rezervacijaService.DohvatiListuRezervacijaViewModelAsync(
            parkingId,
            status,
            datumOd,
            datumDo
        );
        return View(viewModel);
    }


    [HttpGet("rezervacije/moje")]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> MojeRezervacije()
    {
        var korisnik = await _userManager.GetUserAsync(User);
        if (korisnik == null)
            return Challenge();

        var viewModel = await _rezervacijaService.DohvatiMojeRezervacijeViewModelAsync(korisnik.Id);

        // Postavi ViewBag za prikaz statistike
        ViewBag.UkupnoRezervacija = viewModel.UkupnoRezervacija;
        ViewBag.AktivneRezervacije = viewModel.AktivnihRezervacija;
        ViewBag.ZavrseneRezervacije = viewModel.ZavrsenihRezervacija;
        ViewBag.OtkazaneRezervacije = viewModel.OtkazanihRezervacija;

        return View(viewModel);
    }

    [HttpGet("rezervacije/detalji/{id}")]
    public async Task<IActionResult> Detalji(int id)
    {
        var rezervacija = await _rezervacijaService.DohvatiRezervacijuPoIdAsync(id);
        if (rezervacija == null)
        {
            TempData["Greska"] = "Rezervacija nije pronađena.";
            return RedirectToAction(nameof(MojeRezervacije));
        }

        var korisnik = await _userManager.GetUserAsync(User);
        var jeAdmin = User.IsInRole("Administrator") || User.IsInRole("Menadzer");

        if (!jeAdmin && rezervacija.KorisnikId != korisnik?.Id)
            return Forbid();

        var viewModel = await _rezervacijaService.DohvatiDetaljeRezervacijeViewModelAsync(id);
        return View(viewModel);
    }

    [HttpGet("rezervacije/nova")]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> Kreiraj(int? parkingId)
    {
        var viewModel = await _rezervacijaService.DohvatiViewModelZaKreiranjeAsync();

        // Attempt to load from session
        var pendingJson = HttpContext.Session.GetString("PendingRezervacija");
        if (!string.IsNullOrEmpty(pendingJson))
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var savedModel = JsonSerializer.Deserialize<RezervacijaKreirajViewModel>(pendingJson, options);
                if (savedModel != null)
                {
                    viewModel.ParkingId = savedModel.ParkingId;
                    viewModel.PocetakRezervacije = savedModel.PocetakRezervacije;
                    viewModel.KrajRezervacije = savedModel.KrajRezervacije;
                    viewModel.ParkingMjestoId = savedModel.ParkingMjestoId;
                }
            }
            catch
            {
                // Ignore parse errors
            }
        }

        if (parkingId.HasValue)
        {
            viewModel.ParkingId = parkingId.Value;
        }

        return View(viewModel);
    }

    [HttpPost("rezervacije/nova")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> Kreiraj(RezervacijaKreirajViewModel model)
    {
        var parking = await _parkingService.DohvatiParkingPoIdAsync(model.ParkingId);
        if (parking != null)
        {
            model.CijenaPoSatu = parking.CijenaPoSatu;
            ModelState.Remove(nameof(model.CijenaPoSatu));
        }

        if (!ModelState.IsValid)
        {
            model.DostupniParkinzi = (
                await _rezervacijaService.DohvatiViewModelZaKreiranjeAsync()
            ).DostupniParkinzi;
            return View(model);
        }

        try
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik != null)
            {
                if (await _rezervacijaService.KorisnikImaAktivnuRezervacijuUPerioduAsync(korisnik.Id, model.PocetakRezervacije, model.KrajRezervacije))
                {
                    ModelState.AddModelError("", "Već imate aktivnu rezervaciju koja se preklapa sa ovim terminom.");
                    model.DostupniParkinzi = (
                        await _rezervacijaService.DohvatiViewModelZaKreiranjeAsync()
                    ).DostupniParkinzi;
                    return View(model);
                }
            }

            // Provjeri dostupnost PRIJE plaćanja
            if (model.ParkingMjestoId.HasValue)
            {
                // Korisnik je odabrao specificno mjesto — provjeri da li je to mjesto slobodno
                if (!await _rezervacijaService.ProvjeriDostupnostMjestaAsync(
                        model.ParkingMjestoId.Value, model.PocetakRezervacije, model.KrajRezervacije))
                {
                    ModelState.AddModelError("", "Odabrano parking mjesto nije dostupno u odabranom terminu.");
                    model.DostupniParkinzi = (
                        await _rezervacijaService.DohvatiViewModelZaKreiranjeAsync()
                    ).DostupniParkinzi;
                    return View(model);
                }
            }
            else
            {
                // Automatski odabir — provjeri da li postoji slobodno mjesto na parkingu
                var slobodnoMjesto = await _rezervacijaService.DohvatiPrvoSlobodnoMjestoAsync(
                    model.ParkingId, model.PocetakRezervacije, model.KrajRezervacije);
                if (slobodnoMjesto == null)
                {
                    ModelState.AddModelError("", "Nema slobodnih parking mjesta u odabranom terminu.");
                    model.DostupniParkinzi = (
                        await _rezervacijaService.DohvatiViewModelZaKreiranjeAsync()
                    ).DostupniParkinzi;
                    return View(model);
                }
            }

            // Spremi podatke rezervacije u session — rezervacija se kreira tek nakon plaćanja
            var options = new JsonSerializerOptions { WriteIndented = false };
            // Resetuj SelectList (nije serijalizabilan)
            model.DostupniParkinzi = null;
            model.DostupnaParkingMjesta = new();
            var json = JsonSerializer.Serialize(model, options);
            HttpContext.Session.SetString("PendingRezervacija", json);

            return RedirectToAction(nameof(Placanje));
        }
        catch (InvalidOperationException greska)
        {
            ModelState.AddModelError("", greska.Message);
            model.DostupniParkinzi = (
                await _rezervacijaService.DohvatiViewModelZaKreiranjeAsync()
            ).DostupniParkinzi;
            return View(model);
        }
    }


    [HttpGet("rezervacije/placanje")]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> Placanje()
    {
        var json = HttpContext.Session.GetString("PendingRezervacija");
        if (string.IsNullOrEmpty(json))
        {
            TempData["Greska"] = "Nema podataka o rezervaciji. Molimo počnite ispočetka.";
            return RedirectToAction(nameof(Kreiraj));
        }

        var pendingModel = JsonSerializer.Deserialize<RezervacijaKreirajViewModel>(json);
        if (pendingModel == null)
        {
            TempData["Greska"] = "Greška pri učitavanju rezervacije. Molimo počnite ispočetka.";
            return RedirectToAction(nameof(Kreiraj));
        }

        var korisnik = await _userManager.GetUserAsync(User);
        if (korisnik != null && await _rezervacijaService.KorisnikImaAktivnuRezervacijuUPerioduAsync(korisnik.Id, pendingModel.PocetakRezervacije, pendingModel.KrajRezervacije))
        {
            TempData["Greska"] = "Već imate aktivnu rezervaciju koja se preklapa sa ovim terminom.";
            return RedirectToAction(nameof(Kreiraj));
        }

        var parking = await _parkingService.DohvatiParkingPoIdAsync(pendingModel.ParkingId);

        var viewModel = new RezervacijaPlacanjeViewModel
        {
            RezervacijaId = 0, // Rezervacija još nije kreirana
            ParkingNaziv = parking?.Naziv ?? "Nepoznat parking",
            PocetakRezervacije = pendingModel.PocetakRezervacije,
            KrajRezervacije = pendingModel.KrajRezervacije,
            BrojSati = pendingModel.BrojSati,
            CijenaPoSatu = pendingModel.CijenaPoSatu,
            UkupnaCijena = pendingModel.UkupnaCijena,
            Popust = pendingModel.Popust,
        };

        return View(viewModel);
    }

    [HttpPost("rezervacije/placanje")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> PlacanjePotvrdji(PlacanjeViewModel model)
    {
        var json = HttpContext.Session.GetString("PendingRezervacija");
        if (string.IsNullOrEmpty(json))
        {
            TempData["Greska"] = "Sesija je istekla. Molimo počnite ispočetka.";
            return RedirectToAction(nameof(Kreiraj));
        }

        var pendingModel = JsonSerializer.Deserialize<RezervacijaKreirajViewModel>(json);
        if (pendingModel == null)
        {
            TempData["Greska"] = "Greška pri obradi rezervacije. Molimo počnite ispočetka.";
            return RedirectToAction(nameof(Kreiraj));
        }

        try
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
                return Challenge();

            // Plati je potvrđeno — sada kreiraj rezervaciju i pošalji email
            var rezervacija = await _rezervacijaService.KreirajRezervacijuAsync(pendingModel, korisnik.Id);

            // Obriši privremene podatke iz sessiona
            HttpContext.Session.Remove("PendingRezervacija");

            TempData["Uspjeh"] = "Plaćanje je uspješno! Rezervacija je kreirana i potvrda je poslana na vaš email.";
            return RedirectToAction("Show", "QRKod", new { id = rezervacija.RezervacijaId });
        }
        catch (InvalidOperationException greska)
        {
            TempData["Greska"] = $"Greška pri kreiranju rezervacije: {greska.Message}";
            return RedirectToAction(nameof(Kreiraj));
        }
        catch (Exception)
        {
            TempData["Greska"] = "Došlo je do greške. Molimo pokušajte ponovo.";
            return RedirectToAction(nameof(Kreiraj));
        }
    }


    [HttpGet("rezervacije/produzi/{id}")]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> Produzi(int id)
    {
        var rezervacija = await _rezervacijaService.DohvatiRezervacijuPoIdAsync(id);
        if (rezervacija == null)
        {
            TempData["Greska"] = "Rezervacija nije pronađena.";
            return RedirectToAction(nameof(MojeRezervacije));
        }

        var korisnik = await _userManager.GetUserAsync(User);
        if (rezervacija.KorisnikId != korisnik?.Id)
            return Forbid();

        var viewModel = new RezervacijaProduziViewModel
        {
            RezervacijaId = rezervacija.RezervacijaId,
            ParkingNaziv = rezervacija.Parking?.Naziv ?? "Nepoznat",
            PocetakRezervacije = rezervacija.PocetakRezervacije,
            KrajRezervacije = rezervacija.KrajRezervacije,
            UkupnaCijena = rezervacija.UkupnaCijena,
            CijenaPoSatu = rezervacija.Parking?.CijenaPoSatu ?? 0,
        };

        return View(viewModel);
    }

    [HttpPost("rezervacije/produzi/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> ProduziPotvrdi(int id, int dodatnoVrijeme)
    {
        try
        {
            var rezervacija = await _rezervacijaService.DohvatiRezervacijuPoIdAsync(id);
            if (rezervacija == null)
            {
                TempData["Greska"] = "Rezervacija nije pronađena.";
                return RedirectToAction(nameof(MojeRezervacije));
            }

            var cijenaPoSatu = rezervacija.Parking?.CijenaPoSatu ?? 0;
            var dodatnaCijena = cijenaPoSatu * (dodatnoVrijeme / 60m);

            await _rezervacijaService.ProduziRezervacijuAsync(id, dodatnoVrijeme);

            TempData["Uspjeh"] = $"Rezervacija je produžena za {dodatnoVrijeme} minuta. Dodatna cijena: {dodatnaCijena:F2} KM";
            return RedirectToAction(nameof(MojeRezervacije));
        }
        catch (Exception ex)
        {
            TempData["Greska"] = $"Greška pri produženju: {ex.Message}";
            return RedirectToAction(nameof(MojeRezervacije));
        }
    }

    [HttpGet("rezervacije/otkazi/{id}")]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> Otkazi(int id)
    {
        var rezervacija = await _rezervacijaService.DohvatiRezervacijuPoIdAsync(id);
        if (rezervacija == null)
        {
            TempData["Greska"] = "Rezervacija nije pronađena.";
            return RedirectToAction(nameof(MojeRezervacije));
        }

        var korisnik = await _userManager.GetUserAsync(User);
        if (rezervacija.KorisnikId != korisnik?.Id)
            return Forbid();

        var viewModel = new RezervacijaOtkaziViewModel
        {
            RezervacijaId = rezervacija.RezervacijaId,
            ParkingNaziv = rezervacija.Parking?.Naziv ?? "Nepoznat",
            PocetakRezervacije = rezervacija.PocetakRezervacije,
            KrajRezervacije = rezervacija.KrajRezervacije,
            UkupnaCijena = rezervacija.UkupnaCijena,
        };

        return View(viewModel);
    }

    [HttpPost("rezervacije/otkazi/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> OtkaziPotvrdi(int id, RezervacijaOtkaziViewModel model)
    {
        model.RezervacijaId = id;

        var rezultat = await _rezervacijaService.OtkaziRezervacijuAsync(model);
        if (rezultat)
        {
            TempData["Uspjeh"] = "Rezervacija je uspješno otkazana!";
            return RedirectToAction(nameof(MojeRezervacije));
        }

        TempData["Greska"] = "Greška pri otkazivanju rezervacije.";
        return RedirectToAction(nameof(MojeRezervacije));
    }

    [HttpGet("rezervacije/qr-kod/{id}")]
    public async Task<IActionResult> PrikaziQRKod(int id)
    {
        var rezervacija = await _rezervacijaService.DohvatiRezervacijuPoIdAsync(id);
        if (rezervacija == null)
        {
            TempData["Greska"] = "Rezervacija nije pronađena.";
            return RedirectToAction(nameof(MojeRezervacije));
        }

        var korisnik = await _userManager.GetUserAsync(User);
        var jeAdmin = User.IsInRole("Administrator") || User.IsInRole("Menadzer");

        if (!jeAdmin && rezervacija.KorisnikId != korisnik?.Id)
            return Forbid();

        var viewModel = await _rezervacijaService.DohvatiQRKodZaRezervacijuAsync(id);
        return View(viewModel);
    }


    [HttpGet("rezervacije/uredi/{id}")]
    [Authorize(Roles = "Administrator,Menadzer")]
    public async Task<IActionResult> Uredi(int id)
    {
        var viewModel = await _rezervacijaService.DohvatiViewModelZaUredjivanjeAsync(id);
        if (viewModel == null)
        {
            TempData["Greska"] = "Rezervacija nije pronađena.";
            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    [HttpPost("rezervacije/uredi/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Menadzer")]
    public async Task<IActionResult> Uredi(int id, RezervacijaUrediViewModel model)
    {
        if (id != model.RezervacijaId)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _rezervacijaService.AzurirajRezervacijuAsync(model);
            TempData["Uspjeh"] = "Rezervacija je uspješno ažurirana!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException greska)
        {
            ModelState.AddModelError("", greska.Message);
            return View(model);
        }
    }

    [HttpPost("rezervacije/obrisi/{id}")]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id)
    {
        var rezultat = await _rezervacijaService.ObrisiRezervacijuAsync(id);
        if (rezultat)
            TempData["Uspjeh"] = "Rezervacija je uspješno obrisana!";
        else
            TempData["Greska"] = "Rezervacija nije pronađena.";

        return RedirectToAction(nameof(Index));
    }

    // Api za dohvat slobodnih mjesta

    [HttpGet("rezervacije/slobodna-mjesta")]
    public async Task<IActionResult> DohvatiSlobodnaMjestaZaPeriod(int parkingId, DateTime pocetak, DateTime kraj)
    {
        var mjesta = await _rezervacijaService.DohvatiSlobodnaMjestaZaPeriodAsync(parkingId, pocetak, kraj);
        return Json(mjesta.Select(m => new { value = m.ParkingMjestoId, text = $"Mjesto {m.BrojMjesta}" }));
    }
}
