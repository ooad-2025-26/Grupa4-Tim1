using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Korisnik;
using smartPark.Models.ViewModels.Korisnik.Admin;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers
{
    [Authorize]
    public class KorisnikController : Controller
    {
        private readonly IKorisnikService _korisnikServis;
        private readonly SignInManager<Korisnik> _signInManager;
        private readonly UserManager<Korisnik> _userManager;

        public KorisnikController(
            IKorisnikService korisnikServis,
            SignInManager<Korisnik> signInManager,
            UserManager<Korisnik> userManager
        )
        {
            _korisnikServis = korisnikServis;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Login

        [HttpGet("prijava")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("MojProfil");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost("prijava")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/")
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Login.cshtml", model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Nalog je zaključan. Pokušajte kasnije.");
                return View("~/Views/Account/Login.cshtml", model);
            }

            ModelState.AddModelError("", "Neispravan email ili lozinka.");
            return View("~/Views/Account/Login.cshtml", model);
        }

        // Registracija

        [HttpGet("registracija")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost("registracija")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Register.cshtml", model);
            }

            var korisnik = new Korisnik
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Aktivan = true,
                DatumRegistracije = DateTime.Now,
                BrojVozacke = model.BrojVozacke,
            };

            var result = await _userManager.CreateAsync(korisnik, model.Password);

            if (result.Succeeded)
            {
                // Dodijeli ulogu "Vozac" novom korisniku
                await _userManager.AddToRoleAsync(korisnik, "Vozac");

                // Prijavi korisnika
                await _signInManager.SignInAsync(korisnik, isPersistent: false);

                TempData["Uspjeh"] = "Uspješno ste registrovani!";
                return RedirectToAction("MojProfil");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("~/Views/Account/Register.cshtml", model);
        }

        // Odjava

        [HttpPost("odjava")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Korisnik");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("MojProfil");
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/korisnici")]
        public async Task<IActionResult> Index(string? uloga, string? status, string? pretraga)
        {
            var viewModel = await _korisnikServis.DohvatiAdminListuKorisnikaAsync(uloga, status, pretraga);
            return View("~/Views/Admin/Users.cshtml", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/korisnici/detalji/{id}")]
        public async Task<IActionResult> Detalji(string id)
        {
            var viewModel = await _korisnikServis.DohvatiAdminDetaljeKorisnikaAsync(id);
            if (viewModel == null)
                return NotFound();

            return View("~/Views/Admin/Detalji.cshtml", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/korisnici/dodaj")]
        public async Task<IActionResult> Kreiraj()
        {
            var viewModel = await _korisnikServis.DohvatiAdminViewModelZaKreiranjeAsync();
            return View("~/Views/Admin/Kreiraj.cshtml", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("admin/korisnici/dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kreiraj(AdminKorisnikKreirajViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DostupneUloge = await _korisnikServis.DohvatiSveUlogeZaSelectListAsync();
                model.DostupniParkinzi =
                    await _korisnikServis.DohvatiSveParkingeZaSelectListAsync();
                return View("~/Views/Admin/Kreiraj.cshtml", model);
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
            return View("~/Views/Admin/Kreiraj.cshtml", model);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/korisnici/uredi/{id}")]
        public async Task<IActionResult> Uredi(string id)
        {
            var viewModel = await _korisnikServis.DohvatiAdminViewModelZaUredjivanjeAsync(id);
            if (viewModel == null)
                return NotFound();

            return View("~/Views/Admin/Uredi.cshtml", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("admin/korisnici/uredi/{id}")]
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
                return View("~/Views/Admin/Uredi.cshtml", model);
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
            return View("~/Views/Admin/Uredi.cshtml", model);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("admin/korisnici/zakljucaj/{id}")]
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
        [HttpPost("admin/korisnici/otkljucaj/{id}")]
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
        [HttpPost("admin/korisnici/obrisi/{id}")]
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
        [HttpGet("admin/statistika")]
        public async Task<IActionResult> Statistika()
        {
            var viewModel = await _korisnikServis.DohvatiAdminStatistikuAsync();
            return View("~/Views/Admin/Reports.cshtml", viewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/logovi")]
        public IActionResult Logovi()
        {
            return View("~/Views/Admin/Logovi.cshtml");
        }

        // Menadzer funkcionalnosti

        [Authorize(Roles = "Menadzer")]
        [HttpGet("menadzer/zaposlenici")]
        public async Task<IActionResult> Zaposlenici(string? filter)
        {
            var userId = _korisnikServis.DohvatiTrenutnogKorisnikaId(User);
            var viewModel = await _korisnikServis.DohvatiMenadzerZaposlenikeAsync(userId, filter);
            return View("~/Views/Manager/Zaposlenici.cshtml", viewModel);
        }

        [Authorize(Roles = "Menadzer")]
        [HttpGet("menadzer/radnici")]
        public async Task<IActionResult> Radnici()
        {
            var userId = _korisnikServis.DohvatiTrenutnogKorisnikaId(User);
            var viewModel = await _korisnikServis.DohvatiMenadzerRadnikeAsync(userId);
            return View("~/Views/Manager/Radnici.cshtml", viewModel);
        }

        // Profil

        [Authorize]
        [HttpGet("profil")]
        public async Task<IActionResult> Profil()
        {
            var userId = _korisnikServis.DohvatiTrenutnogKorisnikaId(User);
            var uloga = await _korisnikServis.DohvatiUloguKorisnikaAsync(userId);

            if (uloga == "Vozac")
            {
                var vozacViewModel = await _korisnikServis.DohvatiVozacProfilAsync(userId);
                if (vozacViewModel == null) return NotFound();

                var viewModel = new ProfilViewModel
                {
                    Id = vozacViewModel.Id,
                    Ime = vozacViewModel.Ime,
                    Prezime = vozacViewModel.Prezime,
                    Email = vozacViewModel.Email,
                    Uloga = "Vozac",
                    Aktivan = true,
                    DatumRegistracije = vozacViewModel.DatumRegistracije,
                    BrojVozacke = vozacViewModel.BrojVozacke,
                    BrojRezervacija = vozacViewModel.BrojRezervacija,
                    BrojAktivnihRezervacija = vozacViewModel.BrojAktivnihRezervacija
                };
                return View("~/Views/Profil/Index.cshtml", viewModel);
            }
            else
            {
                var korisnik = await _userManager.FindByIdAsync(userId);
                if (korisnik == null) return NotFound();

                var viewModel = new ProfilViewModel
                {
                    Id = korisnik.Id,
                    Ime = korisnik.Ime,
                    Prezime = korisnik.Prezime,
                    Email = korisnik.Email ?? "",
                    Uloga = uloga ?? "",
                    Aktivan = korisnik.Aktivan,
                    DatumRegistracije = korisnik.DatumRegistracije
                };
                return View("~/Views/Profil/Index.cshtml", viewModel);
            }
        }

        [Authorize]
        [HttpPost("profil/azuriraj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfil(ProfilViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Profil/Index.cshtml", model);
            }

            var korisnik = await _userManager.FindByIdAsync(model.Id);
            if (korisnik == null)
            {
                return NotFound();
            }

            korisnik.Ime = model.Ime;
            korisnik.Prezime = model.Prezime;
            korisnik.Email = model.Email;
            korisnik.NormalizedEmail = model.Email.ToUpper();
            korisnik.UserName = model.Email;
            korisnik.NormalizedUserName = model.Email.ToUpper();
            korisnik.BrojVozacke = model.BrojVozacke;

            var result = await _userManager.UpdateAsync(korisnik);
            if (result.Succeeded)
            {
                TempData["Uspjeh"] = "Profil uspješno ažuriran!";
                return RedirectToAction(nameof(Profil));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("~/Views/Profil/Index.cshtml", model);
        }

        // Moj profil zajednicki za sve uloge

        [HttpGet("moj-profil")]
        public async Task<IActionResult> MojProfil()
        {
            return RedirectToAction(nameof(Profil));
        }
    }
}
