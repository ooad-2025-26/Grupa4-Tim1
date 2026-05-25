using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Izvjestaj;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers
{
    [Authorize(Roles = "Administrator,Menadzer")]
    public class IzvjestajController : Controller
    {
        private readonly IIzvjestajService _izvjestajService;

        public IzvjestajController(IIzvjestajService izvjestajService)
        {
            _izvjestajService = izvjestajService;
        }


        [HttpGet("izvjestaj")]
        public async Task<IActionResult> Index(int? parkingId, TipIzvjestaja? tip)
        {
            var viewModel = await _izvjestajService.DohvatiListuIzvjestajaViewModelAsync(
                parkingId,
                tip
            );
            return View(viewModel);
        }

    
        [HttpGet("izvjestaj/detalji/{id}")]
        public async Task<IActionResult> Detalji(int id)
        {
            var viewModel = await _izvjestajService.DohvatiDetaljeIzvjestajaViewModelAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        // Kreiranje izvjestaja

        [HttpGet("izvjestaj/dodaj")]
        public async Task<IActionResult> Kreiraj()
        {
            var viewModel = await _izvjestajService.DohvatiViewModelZaKreiranjeAsync();
            return View(viewModel);
        }

        [HttpPost("izvjestaj/dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kreiraj(IzvjestajKreirajViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DostupniParkinzi = (
                    await _izvjestajService.DohvatiViewModelZaKreiranjeAsync()
                ).DostupniParkinzi;
                return View(model);
            }

            try
            {
                var izvjestaj = await _izvjestajService.GenerisiIzvjestajAsync(model);
                TempData["Uspjeh"] = "Izvještaj uspješno generisan!";

                if (model.GenerisiPdf)
                {
                    var pdf = await _izvjestajService.GenerisiPdfIzvjestajAsync(
                        izvjestaj.IzvjestajId
                    );
                    return File(pdf, "application/pdf", $"Izvjestaj_{izvjestaj.IzvjestajId}.pdf");
                }

                if (model.GenerisiExcel)
                {
                    var excel = await _izvjestajService.GenerisiExcelIzvjestajAsync(
                        izvjestaj.IzvjestajId
                    );
                    return File(
                        excel,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Izvjestaj_{izvjestaj.IzvjestajId}.xlsx"
                    );
                }

                return RedirectToAction(nameof(Detalji), new { id = izvjestaj.IzvjestajId });
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.DostupniParkinzi = (
                    await _izvjestajService.DohvatiViewModelZaKreiranjeAsync()
                ).DostupniParkinzi;
                return View(model);
            }
        }


        [HttpPost("izvjestaj/obrisi/{id}")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Obrisi(int id)
        {
            var rezultat = await _izvjestajService.ObrisiIzvjestajAsync(id);
            if (rezultat)
                TempData["Uspjeh"] = "Izvještaj uspješno obrisan!";
            else
                TempData["Greska"] = "Izvještaj nije pronađen.";

            return RedirectToAction(nameof(Index));
        }

        // Popunjenost

        [HttpGet("izvjestaj/generisi-popunjenost")]
        public async Task<IActionResult> GenerisiPopunjenost(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            try
            {
                var izvjestaj = await _izvjestajService.GenerisiPopunjenostIzvjestajAsync(
                    parkingId,
                    od,
                    doo
                );
                return Json(izvjestaj);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Prihodi

        [HttpGet("izvjestaj/generisi-prihode")]
        public async Task<IActionResult> GenerisiPrihode(int parkingId, DateTime od, DateTime doo)
        {
            try
            {
                var izvjestaj = await _izvjestajService.GenerisiPrihodiIzvjestajAsync(
                    parkingId,
                    od,
                    doo
                );
                return Json(izvjestaj);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Korisnici

        [HttpGet("izvjestaj/generisi-korisnike")]
        public async Task<IActionResult> GenerisiKorisnike(int parkingId, DateTime od, DateTime doo)
        {
            try
            {
                var izvjestaj = await _izvjestajService.GenerisiKorisniciIzvjestajAsync(
                    parkingId,
                    od,
                    doo
                );
                return Json(izvjestaj);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
