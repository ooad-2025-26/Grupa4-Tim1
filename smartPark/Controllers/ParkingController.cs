using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Parking;
using smartPark.Models.ViewModels.Parking.Admin;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers;

[Authorize]
public class ParkingController : Controller
{
    private readonly IParkingService _parkingService;

    public ParkingController(IParkingService parkingService)
    {
        _parkingService = parkingService;
    }


    [Authorize(Roles = "Administrator,Menadzer")]
    [HttpGet("parking")]
    public async Task<IActionResult> Index()
    {
        var parkinzi = await _parkingService.DohvatiSveParkingeAsync();
        return View("~/Views/Manager/Parkings.cshtml", parkinzi);
    }

    // MAPA PARKINGA

    [HttpGet("parking/mapa")]
    public async Task<IActionResult> Map()
    {
        var parkinzi = await _parkingService.DohvatiSveParkingeAsync();
        return View(parkinzi);
    }


    [HttpGet("parking/detalji/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var parking = await _parkingService.DohvatiParkingPoIdAsync(id);
        if (parking == null)
        {
            TempData["Greska"] = "Parking nije pronađen.";
            return RedirectToAction(nameof(Map));
        }

        // Kreiraj ViewModel za detalje
        var viewModel = new ParkingDetailsViewModel
        {
            ParkingId = parking.ParkingId,
            Naziv = parking.Naziv,
            Adresa = parking.Adresa,
            UkupnoMjesta = parking.UkupnoMjesta,
            SlobodnaMjesta = parking.SlobodnaMjesta,
            CijenaPoSatu = parking.CijenaPoSatu,
            TipParkinga = parking.TipParkinga,
            Aktivan = parking.Aktivan,
            Latitude = parking.Latitude,
            Longitude = parking.Longitude,
        };

        return View(viewModel);
    }


    [Authorize(Roles = "Administrator,Menadzer")]
    [HttpGet("parking/dodaj")]
    public async Task<IActionResult> Create()
    {
        var viewModel = await _parkingService.DohvatiAdminViewModelZaKreiranjeAsync();
        if (User.IsInRole("Menadzer"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
                viewModel.MenadzerId = userId;
        }
        return View(viewModel);
    }

    [Authorize(Roles = "Administrator,Menadzer")]
    [HttpPost("parking/dodaj")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminParkingKreirajViewModel model)
    {
        if (User.IsInRole("Menadzer"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
                model.MenadzerId = userId;
        }

        if (!ModelState.IsValid)
        {
            model.DostupniMenadzeri = await _parkingService.DohvatiSveMenadzereZaSelectListAsync();
            await _parkingService.PopuniCjenovnikeZaKreirajAsync(model);
            return View(model);
        }

        try
        {
            await _parkingService.AdminKreirajParkingAsync(model);
            TempData["Uspjeh"] = "Parking uspješno kreiran!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.DostupniMenadzeri = await _parkingService.DohvatiSveMenadzereZaSelectListAsync();
            await _parkingService.PopuniCjenovnikeZaKreirajAsync(model);
            return View(model);
        }
    }


    [Authorize(Roles = "Administrator,Menadzer")]
    [HttpGet("parking/uredi/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var parking = await _parkingService.DohvatiParkingPoIdAsync(id);
        if (parking == null)
        {
            TempData["Greska"] = "Parking nije pronađen.";
            return RedirectToAction(nameof(Index));
        }

        if (User.IsInRole("Menadzer"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || parking.MenadzerID == null || !parking.MenadzerID.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(userId))
            {
                return Forbid();
            }
        }

        var viewModel = new AdminParkingUrediViewModel
        {
            ParkingId = parking.ParkingId,
            Naziv = parking.Naziv,
            Adresa = parking.Adresa,
            UkupnoMjesta = parking.UkupnoMjesta,
            SlobodnaMjesta = parking.SlobodnaMjesta,
            CijenaPoSatu = parking.CijenaPoSatu,
            TipParkinga = parking.TipParkinga,
            Aktivan = parking.Aktivan,
            Latitude = parking.Latitude,
            Longitude = parking.Longitude,
            RadnoVrijeme = parking.RadnoVrijeme,
            MenadzerId = !string.IsNullOrEmpty(parking.MenadzerID) ? parking.MenadzerID.Split(',').FirstOrDefault() : null,
            DefaultniCjenovnikId = parking.DefaultniCjenovnikId,
            DnevniCjenovnikId = parking.DnevniCjenovnikId,
            NocniCjenovnikId = parking.NocniCjenovnikId,
            DostupniMenadzeri = await _parkingService.DohvatiSveMenadzereZaSelectListAsync(),
        };

        await _parkingService.PopuniCjenovnikeZaUrediAsync(viewModel);

        return View(viewModel);
    }

    [Authorize(Roles = "Administrator,Menadzer")]
    [HttpPost("parking/uredi/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminParkingUrediViewModel model)
    {
        if (id != model.ParkingId)
            return NotFound();

        var parking = await _parkingService.DohvatiParkingPoIdAsync(id);
        if (parking == null)
            return NotFound();

        if (User.IsInRole("Menadzer"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || parking.MenadzerID == null || !parking.MenadzerID.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(userId))
            {
                return Forbid();
            }
            model.MenadzerId = userId;
        }

        if (!ModelState.IsValid)
        {
            model.DostupniMenadzeri = await _parkingService.DohvatiSveMenadzereZaSelectListAsync();
            await _parkingService.PopuniCjenovnikeZaUrediAsync(model);
            return View(model);
        }

        try
        {
            await _parkingService.AdminAzurirajParkingAsync(model);
            TempData["Uspjeh"] = "Parking uspješno ažuriran!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.DostupniMenadzeri = await _parkingService.DohvatiSveMenadzereZaSelectListAsync();
            await _parkingService.PopuniCjenovnikeZaUrediAsync(model);
            return View(model);
        }
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet("parking/obrisi/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var parking = await _parkingService.DohvatiParkingPoIdAsync(id);
        if (parking == null)
        {
            TempData["Greska"] = "Parking nije pronađen.";
            return RedirectToAction(nameof(Index));
        }

        return View(parking);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost("parking/obrisi/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _parkingService.AdminObrisiParkingAsync(id);
        if (result)
        {
            TempData["Uspjeh"] = "Parking uspješno obrisan!";
        }
        else
        {
            TempData["Greska"] = "Parking nije pronađen.";
        }
        return RedirectToAction(nameof(Index));
    }
}
