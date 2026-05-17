using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.ViewModels.Korisnik.Admin;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers
{
    [Authorize]
    public class KorisnikController : Controller
    {
        private readonly IKorisnikService _korisnikServis;

        public KorisnikController(IKorisnikService korisnikServis)
        {
            _korisnikServis = korisnikServis;
        }

        // SAMO ADMIN IMA PRISTUP OVIM AKCIJAMA

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Index(string? uloga, string? status)
        {
            var viewModel = await _korisnikServis.DohvatiAdminListuKorisnikaAsync(uloga, status);
            return View("Admin/Index", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Detalji(string id)
        {
            var viewModel = await _korisnikServis.DohvatiAdminDetaljeKorisnikaAsync(id);
            if (viewModel == null)
                return NotFound();

            return View("Admin/Detalji", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Kreiraj()
        {
            var viewModel = await _korisnikServis.DohvatiAdminViewModelZaKreiranjeAsync();
            return View("Admin/Kreiraj", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kreiraj(AdminKorisnikKreirajViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DostupneUloge = await _korisnikServis.DohvatiSveUlogeZaSelectListAsync();
                model.DostupniParkinzi =
                    await _korisnikServis.DohvatiSveParkingeZaSelectListAsync();
                return View("Admin/Kreiraj", model);
            }

            var result = await _korisnikServis.AdminKreirajKorisnikaAsync(model);

            if (result.Uspjeh)
            {
                TempData["Uspjeh"] = $"Korisnik {model.Ime} {model.Prezime} uspješno kreiran!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var greska in result.Greske)
            {
                ModelState.AddModelError("", greska);
            }

            model.DostupneUloge = await _korisnikServis.DohvatiSveUlogeZaSelectListAsync();
            model.DostupniParkinzi = await _korisnikServis.DohvatiSveParkingeZaSelectListAsync();
            return View("Admin/Kreiraj", model);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Uredi(string id)
        {
            var viewModel = await _korisnikServis.DohvatiAdminViewModelZaUredjivanjeAsync(id);
            if (viewModel == null)
                return NotFound();

            return View("Admin/Uredi", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Uredi(string id, AdminKorisnikUrediViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.DostupneUloge = await _korisnikServis.DohvatiSveUlogeZaSelectListAsync();
                model.DostupniParkinzi =
                    await _korisnikServis.DohvatiSveParkingeZaSelectListAsync();
                return View("Admin/Uredi", model);
            }

            var result = await _korisnikServis.AdminAzurirajKorisnikaAsync(model);

            if (result.Uspjeh)
            {
                TempData["Uspjeh"] = "Korisnik uspješno ažuriran!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var greska in result.Greske)
            {
                ModelState.AddModelError("", greska);
            }

            model.DostupneUloge = await _korisnikServis.DohvatiSveUlogeZaSelectListAsync();
            model.DostupniParkinzi = await _korisnikServis.DohvatiSveParkingeZaSelectListAsync();
            return View("Admin/Uredi", model);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Zakljucaj(string id)
        {
            var result = await _korisnikServis.ZakljucajKorisnikaAsync(id);
            if (result.Uspjeh)
                TempData["Uspjeh"] = "Korisnik uspješno zaključan!";
            else
                TempData["Greska"] = result.Greska;

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Otkljucaj(string id)
        {
            var result = await _korisnikServis.OtkljucajKorisnikaAsync(id);
            if (result.Uspjeh)
                TempData["Uspjeh"] = "Korisnik uspješno otključan!";
            else
                TempData["Greska"] = result.Greska;

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Obrisi(string id)
        {
            var result = await _korisnikServis.ObrisiKorisnikaAsync(id);
            if (result.Uspjeh)
                TempData["Uspjeh"] = "Korisnik uspješno obrisan!";
            else
                TempData["Greska"] = result.Greska;

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Statistika()
        {
            var viewModel = await _korisnikServis.DohvatiAdminStatistikuAsync();
            return View("Admin/Statistika", viewModel);
        }

        // SAMO MENADŽER IMA PRISTUP OVIM AKCIJAMA

        [Authorize(Roles = "Menadzer")]
        [HttpGet]
        public async Task<IActionResult> Zaposlenici(string? filter)
        {
            var viewModel = await _korisnikServis.DohvatiMenadzerZaposlenikeAsync(filter);
            return View("Menadzer/Zaposlenici", viewModel);
        }

        [Authorize(Roles = "Menadzer")]
        [HttpGet]
        public async Task<IActionResult> Radnici()
        {
            var viewModel = await _korisnikServis.DohvatiMenadzerRadnikeAsync();
            return View("Menadzer/Radnici", viewModel);
        }

        // SAMO VOZAČ IMA PRISTUP OVIM AKCIJAMA

        [Authorize(Roles = "Vozac")]
        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var userId = _korisnikServis.DohvatiTrenutnogKorisnikaId(User);
            var viewModel = await _korisnikServis.DohvatiVozacProfilAsync(userId);
            return View("Vozac/Profil", viewModel);
        }

        // SVI PRIJAVLJENI KORISNICI IMAJU PRISTUP

        [HttpGet]
        public async Task<IActionResult> MojProfil()
        {
            var userId = _korisnikServis.DohvatiTrenutnogKorisnikaId(User);
            var uloga = await _korisnikServis.DohvatiUloguKorisnikaAsync(userId);

            // Redirekcija na odgovarajući profil prema ulozi
            return uloga switch
            {
                "Administrator" => RedirectToAction(nameof(Statistika)),
                "Menadzer" => RedirectToAction(nameof(Zaposlenici)),
                "Vozac" => RedirectToAction(nameof(Profil)),
                _ => RedirectToAction("Index", "Home"),
            };
        }
    }
}
