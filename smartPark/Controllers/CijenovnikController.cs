using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Cjenovnik;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers
{
    [Authorize(Roles = "Administrator,Menadzer")]
    public class CjenovnikController : Controller
    {
        private readonly ICjenovnikService _cjenovnikServis;
        private readonly IParkingService _parkingServis;

        public CjenovnikController(ICjenovnikService cjenovnikServis, IParkingService parkingServis)
        {
            _cjenovnikServis = cjenovnikServis;
            _parkingServis = parkingServis;
        }

        [HttpGet("cjenovnik")]
        public async Task<IActionResult> Index(int? parkingId)
        {
            var viewModel = await _cjenovnikServis.DohvatiListuCjenovnikaViewModelAsync(parkingId);
            return View(viewModel);
        }

        [HttpGet("cjenovnik/detalji/{id}")]
        public async Task<IActionResult> Detalji(int id)
        {
            var viewModel = await _cjenovnikServis.DohvatiDetaljeCjenovnikaViewModelAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        // Kreiranje cjenovnika (admin, manadzer)

        [HttpGet("cjenovnik/dodaj")]
        [Authorize(Roles = "Administrator,Menadzer")]
        public async Task<IActionResult> Kreiraj()
        {
            var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
            var viewModel = new CjenovnikKreirajViewModel
            {
                DostupniParkinzi = new[] { new SelectListItem { Value = "", Text = "Opšti cjenovnik (Nije dodijeljen)" } }
                    .Concat(parkinzi.Select(p => new SelectListItem
                    {
                        Value = p.ParkingId.ToString(),
                        Text = $"{p.Naziv} - {p.Adresa}",
                    })),
                DatumPocetka = DateTime.Now.Date,
            };

            return View(viewModel);
        }

        [HttpPost("cjenovnik/dodaj")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Menadzer")]
        public async Task<IActionResult> Kreiraj(CjenovnikKreirajViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
                model.DostupniParkinzi = new[] { new SelectListItem { Value = "", Text = "Opšti cjenovnik (Nije dodijeljen)" } }
                    .Concat(parkinzi.Select(p => new SelectListItem
                    {
                        Value = p.ParkingId.ToString(),
                        Text = $"{p.Naziv} - {p.Adresa}",
                    }));
                return View(model);
            }

            try
            {
                var cjenovnik = await _cjenovnikServis.KreirajCjenovnikAsync(model);
                TempData["Uspjeh"] = $"Cjenovnik '{model.Naziv}' uspješno kreiran!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException greska)
            {
                ModelState.AddModelError("", greska.Message);

                var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
                model.DostupniParkinzi = new[] { new SelectListItem { Value = "", Text = "Opšti cjenovnik (Nije dodijeljen)" } }
                    .Concat(parkinzi.Select(p => new SelectListItem
                    {
                        Value = p.ParkingId.ToString(),
                        Text = $"{p.Naziv} - {p.Adresa}",
                    }));
                return View(model);
            }
        }

        // Edit cjenovnika (admin, manadzer)

        [HttpGet("cjenovnik/uredi/{id}")]
        [Authorize(Roles = "Administrator,Menadzer")]
        public async Task<IActionResult> Uredi(int id)
        {
            var cjenovnik = await _cjenovnikServis.DohvatiCjenovnikPoIdAsync(id);
            if (cjenovnik == null)
                return NotFound();

            var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
            var viewModel = new CjenovnikUrediViewModel
            {
                CjenovnikId = cjenovnik.CjenovnikId,
                Naziv = cjenovnik.Naziv,
                ParkingId = cjenovnik.ParkingId,
                CijenaDnevna = cjenovnik.CijenaDnevna,
                CijenaNocna = cjenovnik.CijenaNocna,
                Zona = cjenovnik.Zona,
                DatumPocetka = cjenovnik.DatumPocetka,
                DatumKraja = cjenovnik.DatumKraja,
                DostupniParkinzi = new[] { new SelectListItem { Value = "", Text = "Opšti cjenovnik (Nije dodijeljen)" } }
                    .Concat(parkinzi.Select(p => new SelectListItem
                    {
                        Value = p.ParkingId.ToString(),
                        Text = $"{p.Naziv} - {p.Adresa}",
                    })),
            };

            return View(viewModel);
        }

        [HttpPost("cjenovnik/uredi/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Menadzer")]
        public async Task<IActionResult> Uredi(int id, CjenovnikUrediViewModel model)
        {
            if (id != model.CjenovnikId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
                model.DostupniParkinzi = new[] { new SelectListItem { Value = "", Text = "Opšti cjenovnik (Nije dodijeljen)" } }
                    .Concat(parkinzi.Select(p => new SelectListItem
                    {
                        Value = p.ParkingId.ToString(),
                        Text = $"{p.Naziv} - {p.Adresa}",
                    }));
                return View(model);
            }

            try
            {
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
                var parkinzi = await _parkingServis.DohvatiSveParkingeAsync();
                model.DostupniParkinzi = new[] { new SelectListItem { Value = "", Text = "Opšti cjenovnik (Nije dodijeljen)" } }
                    .Concat(parkinzi.Select(p => new SelectListItem
                    {
                        Value = p.ParkingId.ToString(),
                        Text = $"{p.Naziv} - {p.Adresa}",
                    }));
                return View(model);
            }
        }

        // Deaktivacija cjenovnika (admin, manadzer)

        [HttpPost("cjenovnik/deaktiviraj/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Menadzer")]
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

        // Brisanje cjenovnika (admin, menadzer)

        [HttpGet("cjenovnik/obrisi/{id}")]
        [Authorize(Roles = "Administrator,Menadzer")]
        public async Task<IActionResult> Obrisi(int id)
        {
            var cjenovnik = await _cjenovnikServis.DohvatiCjenovnikPoIdAsync(id);
            if (cjenovnik == null)
                return NotFound();

            return View(cjenovnik);
        }

        [HttpPost("cjenovnik/obrisi/{id}")]
        [ActionName("Obrisi")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Menadzer")]
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

        // Api testiranje cjenovnika

        [HttpPost("cjenovnik/primjeni-cijenu")]
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
