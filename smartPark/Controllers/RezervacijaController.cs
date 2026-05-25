using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels;
using smartPark.Models.ViewModels.Rezervacija;
using smartPark.Services.Interfaces;

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
            if (korisnik == null)
                return Challenge();

            var rezervacija = await _rezervacijaService.KreirajRezervacijuAsync(model, korisnik.Id);
            TempData["Uspjeh"] = "Rezervacija je uspješno kreirana!";

            // Preusmjeri na stranicu za plaćanje
            return RedirectToAction("Placanje", "Rezervacija", new { id = rezervacija.RezervacijaId });
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


    [HttpGet("rezervacije/placanje/{id}")]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> Placanje(int id)
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

        var viewModel = new RezervacijaPlacanjeViewModel
        {
            RezervacijaId = rezervacija.RezervacijaId,
            ParkingNaziv = rezervacija.Parking?.Naziv ?? "Nepoznat",
            PocetakRezervacije = rezervacija.PocetakRezervacije,
            BrojSati = (int)
                Math.Ceiling(
                    (rezervacija.KrajRezervacije - rezervacija.PocetakRezervacije).TotalHours
                ),
            CijenaPoSatu = rezervacija.Parking?.CijenaPoSatu ?? 0,
            UkupnaCijena = rezervacija.UkupnaCijena,
        };

        return View(viewModel);
    }

    [HttpPost("rezervacije/placanje/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Vozac")]
    public async Task<IActionResult> Placanje(int id, PlacanjeViewModel model)
    {
        // Ovdje bi išla integracija sa payment gateway-em
        // Za sada samo simuliramo uspješno plaćanje

        TempData["Uspjeh"] = "Plaćanje je uspješno izvršeno!";
        return RedirectToAction("Show", "QRKod", new { id });
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
