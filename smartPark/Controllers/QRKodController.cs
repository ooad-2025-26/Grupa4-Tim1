using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Rezervacija;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers;

[Authorize]
public class QRKodController : Controller
{
    private readonly IRezervacijaService _rezervacijaService;
    private readonly IQRKodService _qrKodService;
    private readonly UserManager<Korisnik> _userManager;

    public QRKodController(
        IRezervacijaService rezervacijaService,
        IQRKodService qrKodService,
        UserManager<Korisnik> userManager
    )
    {
        _rezervacijaService = rezervacijaService;
        _qrKodService = qrKodService;
        _userManager = userManager;
    }

    [HttpGet("qr-kod/prikaz/{id}")]
    public async Task<IActionResult> Show(int id)
    {
        var rezervacija = await _rezervacijaService.DohvatiRezervacijuPoIdAsync(id);
        if (rezervacija == null)
        {
            TempData["Greska"] = "Rezervacija nije pronađena.";
            return RedirectToAction("MojeRezervacije", "Rezervacija");
        }

        // Provjera autorizacije
        var korisnik = await _userManager.GetUserAsync(User);
        var jeAdmin = User.IsInRole("Administrator") || User.IsInRole("Menadzer");

        if (!jeAdmin && rezervacija.KorisnikId != korisnik?.Id)
        {
            return Forbid();
        }

        var qrKodViewModel = await _qrKodService.DohvatiQRKodPoRezervacijiAsync(id);

        if (qrKodViewModel == null)
        {
            // Ako QR kod ne postoji, generiši ga
            qrKodViewModel = await _qrKodService.GenerisiQRKodZaRezervacijuAsync(id);
        }

        ViewBag.Rezervacija = rezervacija;
        return View("~/Views/QR/Show.cshtml", qrKodViewModel);
    }

    [HttpGet("qr-kod/skener")]
    [Authorize(Roles = "Administrator,Menadzer")]
    public IActionResult Scanner()
    {
        return View("~/Views/QR/Scanner.cshtml");
    }

    [HttpPost("qr-kod/validiraj")]
    [AllowAnonymous]
    public async Task<IActionResult> Validiraj([FromBody] ValidirajQRKodDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Kod))
            {
                return Ok(new { uspjeh = false, poruka = "QR kod nije poslan." });
            }

            var validan = await _qrKodService.ValidirajQRKodAsync(dto.Kod);

            if (validan)
            {
                var qrKod = await _qrKodService.DohvatiQRKodPoKoduAsync(dto.Kod);
                if (qrKod == null)
                {
                    return Ok(new { uspjeh = false, poruka = "QR kod nije pronađen." });
                }

                var rezervacija = await _rezervacijaService.DohvatiRezervacijuPoIdAsync(
                    qrKod.RezervacijaId
                );

                return Ok(
                    new
                    {
                        uspjeh = true,
                        rezervacijaId = qrKod.RezervacijaId,
                        parkingNaziv = rezervacija?.Parking?.Naziv,
                        korisnikIme = rezervacija?.Korisnik?.Ime,
                        korisnikPrezime = rezervacija?.Korisnik?.Prezime,
                        pocetak = rezervacija != null ? rezervacija.PocetakRezervacije.ToString("dd.MM.yyyy HH:mm") : null,
                        kraj = rezervacija != null ? rezervacija.KrajRezervacije.ToString("HH:mm") : null,
                    }
                );
            }

            return Ok(new { uspjeh = false, poruka = "Nevažeći ili iskorišteni QR kod." });
        }
        catch (Exception ex)
        {
            return Ok(new { uspjeh = false, poruka = ex.Message });
        }
    }
}

public class ValidirajQRKodDto
{
    public string Kod { get; set; } = string.Empty;
}
