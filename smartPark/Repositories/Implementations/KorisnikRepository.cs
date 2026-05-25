using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Korisnik.Admin;
using smartPark.Repositories.Interfaces;

namespace smartPark.Repositories.Implementations
{
    public class KorisnikRepository : IKorisnikRepository
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _kontekst;

        public KorisnikRepository(
            UserManager<Korisnik> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext kontekst
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _kontekst = kontekst;
        }

        public async Task<Korisnik?> DohvatiPoIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<Korisnik?> DohvatiPoEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IEnumerable<Korisnik>> DohvatiSveAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<IEnumerable<Korisnik>> PronadjiAsync(
            Expression<Func<Korisnik, bool>> uslov
        )
        {
            return await _userManager.Users.Where(uslov).ToListAsync();
        }

        public async Task<IEnumerable<Korisnik>> DohvatiSveSaRezervacijamaAsync()
        {
            return await _userManager.Users.Include(k => k.Rezervacije).ToListAsync();
        }

        public async Task<IEnumerable<Korisnik>> DohvatiSveSaNotifikacijamaAsync()
        {
            return await _userManager.Users.Include(k => k.Notifikacije).ToListAsync();
        }

        public async Task<IdentityResult> DodajAsync(Korisnik korisnik, string lozinka)
        {
            return await _userManager.CreateAsync(korisnik, lozinka);
        }

        public async Task<IdentityResult> AzurirajAsync(Korisnik korisnik)
        {
            return await _userManager.UpdateAsync(korisnik);
        }

        public async Task<IdentityResult> ObrisiAsync(Korisnik korisnik)
        {
            return await _userManager.DeleteAsync(korisnik);
        }

        public async Task<IEnumerable<string>> DohvatiSveRoleAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        }

        public async Task<IEnumerable<string>> DohvatiRoleKorisnikaAsync(Korisnik korisnik)
        {
            return await _userManager.GetRolesAsync(korisnik);
        }

        public async Task<IdentityResult> DodajUloguKorisnikuAsync(Korisnik korisnik, string uloga)
        {
            if (!await _roleManager.RoleExistsAsync(uloga))
            {
                await _roleManager.CreateAsync(new IdentityRole(uloga));
            }
            return await _userManager.AddToRoleAsync(korisnik, uloga);
        }

        public async Task<IdentityResult> UkloniUloguKorisnikuAsync(Korisnik korisnik, string uloga)
        {
            return await _userManager.RemoveFromRoleAsync(korisnik, uloga);
        }

        public async Task<bool> JeLiKorisnikUUloziAsync(Korisnik korisnik, string uloga)
        {
            return await _userManager.IsInRoleAsync(korisnik, uloga);
        }

        public async Task<IEnumerable<Korisnik>> DohvatiKorisnikePoUloziAsync(string uloga)
        {
            var korisnici = await _userManager.Users.ToListAsync();
            var rezultat = new List<Korisnik>();

            foreach (var korisnik in korisnici)
            {
                if (await _userManager.IsInRoleAsync(korisnik, uloga))
                {
                    rezultat.Add(korisnik);
                }
            }
            return rezultat;
        }

        public async Task<Dictionary<string, int>> DohvatiBrojKorisnikaPoUlogamaAsync()
        {
            var rezultat = new Dictionary<string, int>();
            var roleovi = await DohvatiSveRoleAsync();

            foreach (var uloga in roleovi)
            {
                var broj = await PrebrojPoUloziAsync(uloga);
                rezultat.Add(uloga, broj);
            }

            return rezultat;
        }

        public async Task<IdentityResult> ZakljucajKorisnikaAsync(string id)
        {
            var korisnik = await DohvatiPoIdAsync(id);
            if (korisnik == null)
                return IdentityResult.Failed(
                    new IdentityError { Description = "Korisnik nije pronađen" }
                );

            korisnik.Aktivan = false;
            korisnik.LockoutEnabled = true;
            korisnik.LockoutEnd = DateTimeOffset.MaxValue;

            return await _userManager.UpdateAsync(korisnik);
        }

        public async Task<IdentityResult> OtkljucajKorisnikaAsync(string id)
        {
            var korisnik = await DohvatiPoIdAsync(id);
            if (korisnik == null)
                return IdentityResult.Failed(
                    new IdentityError { Description = "Korisnik nije pronađen" }
                );

            korisnik.Aktivan = true;
            korisnik.LockoutEnabled = false;
            korisnik.LockoutEnd = null;

            return await _userManager.UpdateAsync(korisnik);
        }

        public async Task<bool> JeLiKorisnikZakljucanAsync(string id)
        {
            var korisnik = await DohvatiPoIdAsync(id);
            if (korisnik == null)
                return false;

            return korisnik.LockoutEnd.HasValue && korisnik.LockoutEnd > DateTimeOffset.UtcNow;
        }

        public async Task<IEnumerable<Korisnik>> DohvatiZaposlenikePoParkinguAsync(int parkingId)
        {
            // Menadžeri koji su odgovorni za dati parking
            var menadzeri = await _userManager
                .Users.Where(k => k.MenadzerOdgovorniParkingId == parkingId)
                .ToListAsync();

            return menadzeri;
        }

        public async Task<int> DohvatiBrojAktivnihRadnikaDanasAsync(int parkingId)
        {
            var menadzeri = await DohvatiZaposlenikePoParkinguAsync(parkingId);
            return menadzeri.Count(k => k.Aktivan);
        }

        public async Task<int> DohvatiBrojRezervacijaKorisnikaAsync(string korisnikId)
        {
            var korisnik = await DohvatiPoIdAsync(korisnikId);
            if (korisnik == null)
                return 0;

            return await _kontekst.Rezervacije.CountAsync(r => r.KorisnikId == korisnikId);
        }

        public async Task<int> DohvatiBrojAktivnihRezervacijaKorisnikaAsync(string korisnikId)
        {
            return await _kontekst.Rezervacije.CountAsync(r =>
                r.KorisnikId == korisnikId && r.StatusRezervacije == StatusRezervacije.Aktivna
            );
        }

        public async Task<int> DohvatiBrojNotifikacijaKorisnikaAsync(string korisnikId)
        {
            return await _kontekst.Notifikacije.CountAsync(n => n.KorisnikId == korisnikId);
        }

        public async Task<int> DohvatiBrojNecitanihNotifikacijaKorisnikaAsync(string korisnikId)
        {
            return await _kontekst.Notifikacije.CountAsync(n => n.KorisnikId == korisnikId);
        }

        public async Task<bool> PostojiLiSaEmailomAsync(string email, string? izuzmiId = null)
        {
            var korisnik = await _userManager.FindByEmailAsync(email);
            if (korisnik == null)
                return false;

            if (izuzmiId != null && korisnik.Id == izuzmiId)
                return false;

            return true;
        }

        public async Task<int> PrebrojAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        public async Task<int> PrebrojPoUloziAsync(string uloga)
        {
            var korisnici = await DohvatiKorisnikePoUloziAsync(uloga);
            return korisnici.Count();
        }

        public async Task<int> PrebrojAktivneAsync()
        {
            return await _userManager.Users.CountAsync(k => k.Aktivan);
        }

        public async Task<int> PrebrojNeaktivneAsync()
        {
            return await _userManager.Users.CountAsync(k => !k.Aktivan);
        }

        public async Task<int> PrebrojZakljucaneAsync()
        {
            var korisnici = await _userManager.Users.ToListAsync();
            int broj = 0;

            foreach (var k in korisnici)
            {
                if (await JeLiKorisnikZakljucanAsync(k.Id))
                    broj++;
            }

            return broj;
        }

        // Za dropdown listu

        public async Task<IEnumerable<SelectListItem>> DohvatiSveUlogeZaSelectListAsync()
        {
            var roleovi = await DohvatiSveRoleAsync();
            return roleovi.Select(r => new SelectListItem { Value = r, Text = r });
        }

        public async Task<IEnumerable<SelectListItem>> DohvatiSveParkingeZaSelectListAsync()
        {
            var parkinzi = await _kontekst.Parkinzi.ToListAsync();
            return parkinzi.Select(p => new SelectListItem
            {
                Value = p.ParkingId.ToString(),
                Text = $"{p.Naziv} - {p.Adresa}",
            });
        }

        public async Task<Dictionary<DateTime, int>> DohvatiRegistracijePoDanimaAsync(
            int brojDana = 30
        )
        {
            var pocetak = DateTime.Now.AddDays(-brojDana).Date;

            var registracije = await _userManager
                .Users.Where(k => k.DatumRegistracije >= pocetak)
                .GroupBy(k => k.DatumRegistracije.Date)
                .Select(g => new { Datum = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(k => k.Datum, k => k.Broj);

            // Popuni dane bez registracija
            for (int i = 0; i < brojDana; i++)
            {
                var dan = pocetak.AddDays(i);
                if (!registracije.ContainsKey(dan))
                {
                    registracije[dan] = 0;
                }
            }

            return registracije.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<
            List<AdminStatistikaViewModel.NedavnaAktivnost>
        > DohvatiNedavneAktivnostiAsync(int broj = 10)
        {
            var aktivnosti = new List<AdminStatistikaViewModel.NedavnaAktivnost>();

            // Nedavne registracije
            var noveRegistracije = await _userManager
                .Users.OrderByDescending(k => k.DatumRegistracije)
                .Take(broj)
                .ToListAsync();

            foreach (var k in noveRegistracije)
            {
                aktivnosti.Add(
                    new AdminStatistikaViewModel.NedavnaAktivnost
                    {
                        Datum = k.DatumRegistracije,
                        Opis = $"Novi korisnik {k.Ime} {k.Prezime} se registrovao",
                        Tip = "Registracija",
                        Korisnik = k.Email,
                    }
                );
            }

            return aktivnosti.OrderByDescending(a => a.Datum).Take(broj).ToList();
        }
    }
}
