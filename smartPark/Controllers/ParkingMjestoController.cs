using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.ViewModels.ParkingMjesto;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers
{
    [Authorize(Roles = "Administrator,Menadzer")]
    public class ParkingMjestoController : Controller
    {
        private readonly IParkingMjestoService _parkingMjestoService;

        public ParkingMjestoController(IParkingMjestoService parkingMjestoService)
        {
            _parkingMjestoService = parkingMjestoService;
        }

        [HttpGet("parking-mjesto")]
        public async Task<IActionResult> Index(int? parkingId, string? status)
        {
            var viewModel = await _parkingMjestoService.DohvatiListuParkingMjestaViewModelAsync(
                parkingId,
                status
            );
            return View(viewModel);
        }


        [HttpGet("parking-mjesto/detalji/{id}")]
        public async Task<IActionResult> Detalji(int id)
        {
            var viewModel = await _parkingMjestoService.DohvatiDetaljeParkingMjestaViewModelAsync(
                id
            );
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        [HttpGet("parking-mjesto/dodaj")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Kreiraj()
        {
            var viewModel = await _parkingMjestoService.DohvatiViewModelZaKreiranjeAsync();
            return View(viewModel);
        }

        [HttpPost("parking-mjesto/dodaj")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Kreiraj(ParkingMjestoKreirajViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DostupniParkinzi = await _parkingMjestoService
                    .DohvatiViewModelZaKreiranjeAsync()
                    .ContinueWith(t => t.Result.DostupniParkinzi);
                return View(model);
            }

            try
            {
                if (model.KreirajViseMjesta && model.BrojZaKreiranje > 1)
                {
                    var kreirana = await _parkingMjestoService.KreirajViseParkingMjestaAsync(model);
                    TempData["Uspjeh"] = $"Uspješno kreirano {kreirana.Count} parking mjesta!";
                }
                else
                {
                    await _parkingMjestoService.KreirajParkingMjestoAsync(model);
                    TempData["Uspjeh"] = "Parking mjesto uspješno kreirano!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException greska)
            {
                ModelState.AddModelError("", greska.Message);
                model.DostupniParkinzi = await _parkingMjestoService
                    .DohvatiViewModelZaKreiranjeAsync()
                    .ContinueWith(t => t.Result.DostupniParkinzi);
                return View(model);
            }
        }

        [HttpGet("parking-mjesto/uredi/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Uredi(int id)
        {
            var viewModel = await _parkingMjestoService.DohvatiViewModelZaUredjivanjeAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        [HttpPost("parking-mjesto/uredi/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Uredi(int id, ParkingMjestoUrediViewModel model)
        {
            if (id != model.ParkingMjestoId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _parkingMjestoService.AzurirajParkingMjestoAsync(model);
                TempData["Uspjeh"] = "Parking mjesto uspješno ažurirano!";
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

        [HttpPost("parking-mjesto/promijeni-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromijeniStatus(
            ParkingMjestoPromjenaStatusaViewModel model
        )
        {
            if (!ModelState.IsValid)
            {
                TempData["Greska"] = "Neispravni podaci za promjenu statusa";
                return RedirectToAction(nameof(Index));
            }

            var rezultat = await _parkingMjestoService.PromijeniStatusAsync(model);
            if (rezultat)
            {
                TempData["Uspjeh"] = $"Status uspješno promijenjen!";
            }
            else
            {
                TempData["Greska"] = "Greška pri promjeni statusa";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("parking-mjesto/oslobodi/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Oslobodi(int id)
        {
            var rezultat = await _parkingMjestoService.OslobodiMjestoAsync(id);
            if (rezultat)
            {
                TempData["Uspjeh"] = "Parking mjesto je oslobođeno!";
            }
            else
            {
                TempData["Greska"] = "Greška pri oslobađanju parking mjesta";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("parking-mjesto/obrisi/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Obrisi(int id)
        {
            var mjesto = await _parkingMjestoService.DohvatiParkingMjestoPoIdAsync(id);
            if (mjesto == null)
                return NotFound();

            return View(mjesto);
        }

        [HttpPost("parking-mjesto/obrisi/{id}")]
        [ActionName("Obrisi")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ObrisiPotvrda(int id)
        {
            var rezultat = await _parkingMjestoService.ObrisiParkingMjestoAsync(id);
            if (rezultat)
            {
                TempData["Uspjeh"] = "Parking mjesto uspješno obrisano!";
            }
            else
            {
                TempData["Greska"] = "Parking mjesto nije pronađeno.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Api za dohvat slobodnih mjesta

        [HttpGet("parking-mjesto/slobodna")]
        public async Task<IActionResult> DohvatiSlobodnaMjesta(int parkingId)
        {
            var mjesta = await _parkingMjestoService.DohvatiSlobodnaMjestaPoParkinguAsync(
                parkingId
            );
            return Json(mjesta.Select(m => new { m.ParkingMjestoId, m.BrojMjesta }));
        }
    }
}
