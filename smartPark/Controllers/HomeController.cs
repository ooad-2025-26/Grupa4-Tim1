using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Entities;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers;

public class HomeController : Controller
{
    private readonly IKorisnikService _korisnikServis;
    private readonly IParkingService _parkingServis;
    private readonly IRezervacijaService _rezervacijaServis;
    private readonly UserManager<Korisnik> _userManager;
    private readonly SignInManager<Korisnik> _signInManager;
    private readonly IEmailService _emailService;

    public HomeController(
        IKorisnikService korisnikServis,
        IParkingService parkingServis,
        IRezervacijaService rezervacijaServis,
        UserManager<Korisnik> userManager,
        SignInManager<Korisnik> signInManager,
        IEmailService emailService
    )
    {
        _korisnikServis = korisnikServis;
        _parkingServis = parkingServis;
        _rezervacijaServis = rezervacijaServis;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    // Na osnovu rola prikazi odgovarajucu pocetnu stranicu

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Dohvati trenutnog korisnika
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik != null)
            {
                // Ako je admin, idi na admin dashboard
                if (await _userManager.IsInRoleAsync(korisnik, "Administrator"))
                    return RedirectToAction("AdminDashboard", "Home");

                // Ako je menadžer, idi na manager dashboard
                if (await _userManager.IsInRoleAsync(korisnik, "Menadzer"))
                    return RedirectToAction("ManagerDashboard", "Home");
            }

            // Inače, vozač ide na DriverDashboard
            return RedirectToAction("DriverDashboard", "Home");
        }

        // Ako nije prijavljen, idi na login
        return RedirectToAction("Login", "Korisnik");
    }

    // Admin dashboard view

    [Authorize(Roles = "Administrator")]
    [HttpGet("admin/dashboard")]
    public async Task<IActionResult> AdminDashboard()
    {
        var statistika = await _korisnikServis.DohvatiAdminStatistikuAsync();

        // ViewBag za admin dashboard
        ViewBag.UkupnoKorisnika = statistika.UkupnoKorisnika;
        ViewBag.BrojMenadzera = statistika.BrojMenadzera;
        ViewBag.BrojParkinga = statistika.UkupnoParkinga;
        ViewBag.RezervacijeDanas = statistika.UkupnoRezervacija;
        ViewBag.UkupniPrihod = statistika.UkupniPrihod;

        return View("~/Views/Admin/Index.cshtml", statistika);
    }

    // Manadzer dashboard view

    [Authorize(Roles = "Menadzer")]
    [HttpGet("menadzer/dashboard")]
    public async Task<IActionResult> ManagerDashboard()
    {
        var userId = _korisnikServis.DohvatiTrenutnogKorisnikaId(User);
        var statistika = await _parkingServis.DohvatiMenadzerStatistikuParkingaAsync(userId);

        if (statistika == null)
        {
            statistika = new smartPark.Models.ViewModels.Parking.Menadzer.MenadzerParkingStatistikaViewModel
            {
                ParkingId = 0,
                ParkingNaziv = "Nema dodijeljenog parkinga",
                UkupnoParkinga = 0,
                UkupnoMjesta = 0
            };
            ViewBag.NemaParkinga = true;
        }

        // ViewBag za manager dashboard
        ViewBag.BrojParkinga = statistika.UkupnoParkinga;
        ViewBag.UkupnoMjesta = statistika.UkupnoMjesta;
        ViewBag.ProsjecnaPopunjenost = statistika.ProsjecnaZauzetostDanas;
        ViewBag.PrihodDanas = statistika.PrihodDanas;
        ViewBag.ParkingNaziv = statistika.ParkingNaziv;
        ViewBag.ParkingId = statistika.ParkingId;

        return View("~/Views/Manager/Index.cshtml", statistika);
    }

    // Vozac dashboard view

    [Authorize(Roles = "Vozac")]
    [HttpGet("vozac/dashboard")]
    public async Task<IActionResult> DriverDashboard()
    {
        var userId = _korisnikServis.DohvatiTrenutnogKorisnikaId(User);
        var korisnik = await _userManager.FindByIdAsync(userId);

        // ViewBag za vozač dashboard
        ViewBag.KorisnikIme = korisnik?.Ime ?? "vozaču";
        
        var modelRezervacije = await _rezervacijaServis.DohvatiMojeRezervacijeViewModelAsync(userId);
        ViewBag.BrojRezervacija = modelRezervacije.Rezervacije.Count;
        ViewBag.BrojAktivnihRezervacija = modelRezervacije.Rezervacije.Count(r => r.StatusRezervacije == smartPark.Models.Enums.StatusRezervacije.Aktivna);
        
        // Pronađi aktivnu rezervaciju koja je u toku ili nadolazeća
        var aktivna = modelRezervacije.Rezervacije
        .Where(r => r.StatusRezervacije == smartPark.Models.Enums.StatusRezervacije.Aktivna && r.KrajRezervacije > DateTime.Now)
        .OrderBy(r => r.PocetakRezervacije)
        .FirstOrDefault();
            
        ViewBag.AktivnaRezervacija = aktivna;

        return View("_DriverDashboard");
    }

    // 400 Forbidden page

    [HttpGet("/Home/AccessDenied")]
    public IActionResult AccessDenied()
    {
        return View("AccessDenied");
    }

    // Testiranje emaila

    [HttpGet("test-email")]
    [AllowAnonymous]
    public async Task<IActionResult> TestEmail(string adresa = "mhodzic6@etf.unsa.ba")
    {
        try
        {
            await _emailService.PosaljiPotvrduRezervacijeAsync(
                adresa,
                "Mirza Hodžić",
                12345,
                "Kampus UNSA",
                DateTime.Now.AddMinutes(30),
                DateTime.Now.AddHours(2),
                15.50m
            );
            return Content($"Email je uspješno poslan na adresu: {adresa}! Provjeri inbox/spam folder.");
        }
        catch (Exception ex)
        {
            return Content($"Došlo je do greške prilikom slanja emaila: {ex.Message}\n\nDetalji:\n{ex.StackTrace}");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
