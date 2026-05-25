using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Notifikacija;
using smartPark.Services.Interfaces;

namespace smartPark.Controllers
{
    [Authorize(Roles = "Administrator,Menadzer")]
    public class NotifikacijaController : Controller
    {
        private readonly INotifikacijaService _notifikacijaService;
        private readonly UserManager<Korisnik> _userManager;

        public NotifikacijaController(
            INotifikacijaService notifikacijaService,
            UserManager<Korisnik> userManager
        )
        {
            _notifikacijaService = notifikacijaService;
            _userManager = userManager;
        }

        // Slanje notifikacije

        [HttpGet("notifikacija/posalji")]
        public IActionResult Posalji()
        {
            return View(new NotifikacijaPosaljiViewModel());
        }

        [HttpPost("notifikacija/posalji")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Posalji(NotifikacijaPosaljiViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var rezultat = await _notifikacijaService.PosaljiEmailAsync(model);

            if (rezultat)
            {
                TempData["Uspjeh"] = $"Email uspješno poslan na {model.EmailPrimaoca}!";
                return RedirectToAction(nameof(Posalji));
            }

            TempData["Greska"] = "Greška pri slanju emaila. Provjerite konfiguraciju.";
            return View(model);
        }

        // Flooding mail svima

        [HttpGet("notifikacija/posalji-svima")]
        public IActionResult PosaljiSvima()
        {
            return View();
        }

        [HttpPost("notifikacija/posalji-svima")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiSvima(NotifikacijaPosaljiViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var korisnici = _userManager.Users.Where(k => k.EmailConfirmed).ToList();
            var uspjesno = 0;
            var greske = 0;

            foreach (var korisnik in korisnici)
            {
                model.EmailPrimaoca = korisnik.Email ?? string.Empty;
                var rezultat = await _notifikacijaService.PosaljiEmailAsync(model);

                if (rezultat)
                    uspjesno++;
                else
                    greske++;
            }

            TempData["Uspjeh"] = $"Email poslan na {uspjesno} adresa. Greške: {greske}";
            return RedirectToAction(nameof(PosaljiSvima));
        }

        // Mailovi za 4 akcije

        [HttpPost("notifikacija/potvrda")]
        public async Task<IActionResult> PosaljiPotvrduRezervacije(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            decimal cijena
        )
        {
            var rezultat = await _notifikacijaService.PosaljiPotvrduRezervacijeAsync(
                email,
                ime,
                prezime,
                parkingNaziv,
                pocetak,
                kraj,
                cijena
            );

            if (rezultat)
                TempData["Uspjeh"] = "Potvrda rezervacije poslana!";
            else
                TempData["Greska"] = "Greška pri slanju potvrde rezervacije.";

            return RedirectToAction("Detalji", "Rezervacija", new { id = 0 });
        }

        [HttpPost("notifikacija/otkazano")]
        public async Task<IActionResult> PosaljiObavjestenjeOtkazano(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            string razlog
        )
        {
            var rezultat = await _notifikacijaService.PosaljiObavjestenjeOtkazanoAsync(
                email,
                ime,
                prezime,
                parkingNaziv,
                pocetak,
                kraj,
                razlog
            );

            if (rezultat)
                TempData["Uspjeh"] = "Obavještenje o otkazanoj rezervaciji poslano!";
            else
                TempData["Greska"] = "Greška pri slanju obavještenja o otkazanoj rezervaciji.";

            return RedirectToAction("Detalji", "Rezervacija", new { id = 0 });
        }

        // Konfiguracija mail-a

        [HttpGet("notifikacija/testiraj-konfiguraciju")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> TestirajKonfiguraciju()
        {
            var adminEmail = _userManager.GetUserAsync(User).Result?.Email;
            if (string.IsNullOrEmpty(adminEmail))
                adminEmail = "admin@smartpark.com";

            var rezultat = await _notifikacijaService.TestirajEmailKonfiguracijuAsync(adminEmail);

            if (rezultat)
                TempData["Uspjeh"] =
                    $"Test email poslan na {adminEmail}. Provjerite vaš email sandučić.";
            else
                TempData["Greska"] =
                    "Greška pri slanju test emaila. Provjerite email konfiguraciju u appsettings.json.";

            return RedirectToAction(nameof(Posalji));
        }

        // Api test

        [HttpPost("notifikacija/api-posalji")]
        [AllowAnonymous]
        public async Task<IActionResult> ApiPosalji([FromBody] NotifikacijaPosaljiViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rezultat = await _notifikacijaService.PosaljiEmailAsync(model);

            if (rezultat)
                return Ok(
                    new { success = true, message = $"Email poslan na {model.EmailPrimaoca}" }
                );

            return StatusCode(500, new { success = false, message = "Greška pri slanju emaila" });
        }
    }
}
