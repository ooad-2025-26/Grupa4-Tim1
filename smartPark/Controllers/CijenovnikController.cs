using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Cjenovnik;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers
{
    [Authorize(Roles = "Admin,Menadzer")]
    public class CjenovnikController : Controller
    {
        private readonly ICjenovnikService _cjenovnikServis;
        private readonly IParkingService _parkingServis; // Dodaj parking servis za dropdown

        public CjenovnikController(ICjenovnikService cjenovnikServis, IParkingService parkingServis)
        {
            _cjenovnikServis = cjenovnikServis;
            _parkingServis = parkingServis;
        }

        // ========== PRIKAZ SVIH CJENOVNIKA ==========

        [HttpGet]
        public async Task<IActionResult> Index(int? parkingId)
        {
            var viewModel = await _cjenovnikServis.DohvatiListuCjenovnikaViewModelAsync(parkingId);
            return View(viewModel);
        }

        // ========== DETALJI CJENOVNIKA ==========

        [HttpGet]
        public async Task<IActionResult> Detalji(int id)
        {
            var viewModel = await _cjenovnikServis.DohvatiDetaljeCjenovnikaViewModelAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        // ========== KREIRANJE CJENOVNIKA ==========

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Kreiraj()
        {
            // Ručno napravi ViewModel sa listom parkinga
            var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
            var viewModel = new CjenovnikKreirajViewModel
            {
                ParkingLista = parkinzi.Select(p => new SelectListItem
                {
                    Value = p.ParkingId.ToString(),
                    Text = $"{p.Naziv} - {p.Adresa}",
                }),
                DatumPocetka = DateTime.Now.Date,
                TipPerioda = TipPerioda.Dan,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Kreiraj(CjenovnikKreirajViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Ponovo napuni dropdown listu
                var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
                model.ParkingLista = parkinzi.Select(p => new SelectListItem
                {
                    Value = p.ParkingId.ToString(),
                    Text = $"{p.Naziv} - {p.Adresa}",
                });
                return View(model);
            }

            try
            {
                var cjenovnik = await _cjenovnikServis.KreirajCjenovnikAsync(model);
                TempData["Uspjeh"] = $"Cjenovnik za parking uspješno kreiran!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException greska)
            {
                ModelState.AddModelError("", greska.Message);

                // Ponovo napuni dropdown listu
                var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
                model.ParkingLista = parkinzi.Select(p => new SelectListItem
                {
                    Value = p.ParkingId.ToString(),
                    Text = $"{p.Naziv} - {p.Adresa}",
                });
                return View(model);
            }
        }

        // ========== UREDIVANJE CJENOVNIKA ==========

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Uredi(int id)
        {
            var cjenovnik = await _cjenovnikServis.DohvatiCjenovnikPoIdAsync(id);
            if (cjenovnik == null)
                return NotFound();

            var viewModel = new CjenovnikUrediViewModel
            {
                CjenovnikId = cjenovnik.CjenovnikId,
                CijenaPoSatu = cjenovnik.CijenaPoSatu,
                Zona = cjenovnik.Zona,
                TipPerioda = cjenovnik.TipPerioda,
                DatumPocetka = cjenovnik.DatumPocetka,
                DatumKraja = cjenovnik.DatumKraja,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Uredi(int id, CjenovnikUrediViewModel model)
        {
            if (id != model.CjenovnikId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Ažuriranje cijene
                var cjenovnik = await _cjenovnikServis.AzurirajCijenuCjenovnikaAsync(
                    id,
                    model.CijenaPoSatu
                );

                // Ažuriranje ostalih polja
                cjenovnik.Zona = model.Zona;
                cjenovnik.TipPerioda = model.TipPerioda;
                cjenovnik.DatumPocetka = model.DatumPocetka;
                cjenovnik.DatumKraja = model.DatumKraja;

                await _cjenovnikServis.AzurirajCjenovnikAsync(model);

                TempData["Uspjeh"] = "Cjenovnik uspješno ažuriran!";
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

        // ========== DEAKTIVIRANJE CJENOVNIKA ==========

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deaktiviraj(int id)
        {
            try
            {
                var rezultat = await _cjenovnikServis.DeaktivirajCjenovnikAsync(id);
                if (rezultat)
                {
                    TempData["Uspjeh"] = "Cjenovnik uspješno deaktiviran!";
                }
                else
                {
                    TempData["Greska"] = "Cjenovnik nije moguće deaktivirati.";
                }
            }
            catch (InvalidOperationException greska)
            {
                TempData["Greska"] = greska.Message;
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== BRISANJE CJENOVNIKA ==========

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Obrisi(int id)
        {
            var cjenovnik = await _cjenovnikServis.DohvatiCjenovnikPoIdAsync(id);
            if (cjenovnik == null)
                return NotFound();

            return View(cjenovnik);
        }

        [HttpPost]
        [ActionName("Obrisi")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObrisiPotvrda(int id)
        {
            var rezultat = await _cjenovnikServis.ObrisiCjenovnikAsync(id);
            if (rezultat)
            {
                TempData["Uspjeh"] = "Cjenovnik uspješno obrisan!";
            }
            else
            {
                TempData["Greska"] = "Cjenovnik nije pronađen.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== API ENDPOINT ZA PRIMJENU CIJENE ==========

        [HttpPost]
        public async Task<IActionResult> PrimjeniCijenu(int parkingId, int sati, TipPerioda period)
        {
            try
            {
                var ukupnaCijena = await _cjenovnikServis.PrimjeniCijenuCjenovnikaAsync(
                    parkingId,
                    sati,
                    period
                );
                return Json(new { uspjeh = true, cijena = ukupnaCijena });
            }
            catch (Exception greska)
            {
                return Json(new { uspjeh = false, greska = greska.Message });
            }
        }
    }
}
