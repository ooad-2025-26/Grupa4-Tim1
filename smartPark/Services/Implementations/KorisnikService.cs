using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Korisnik.Admin;
using smartPark.Models.ViewModels.Korisnik.Menadzer;
using smartPark.Models.ViewModels.Korisnik.Shared;
using smartPark.Models.ViewModels.Korisnik.Vozac;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class KorisnikService : IKorisnikService
    {
        private readonly IKorisnikRepository _korisnikRepozitorij;
        private readonly IParkingRepository _parkingRepozitorij;

        public KorisnikService(
            IKorisnikRepository korisnikRepozitorij,
            IParkingRepository parkingRepozitorij
        )
        {
            _korisnikRepozitorij = korisnikRepozitorij;
            _parkingRepozitorij = parkingRepozitorij;
        }

        // ========== POMOĆNE RADNJE ==========

        public string DohvatiTrenutnogKorisnikaId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        }

        // ========== ADMIN RADNJE ==========

        public async Task<AdminKorisnikListaViewModel> DohvatiAdminListuKorisnikaAsync(
            string? filterUloga = null,
            string? filterStatus = null
        )
        {
            IEnumerable<Korisnik> korisnici;

            // Filtriraj po ulozi
            if (!string.IsNullOrEmpty(filterUloga))
            {
                korisnici = await _korisnikRepozitorij.DohvatiKorisnikePoUloziAsync(filterUloga);
            }
            else
            {
                korisnici = await _korisnikRepozitorij.DohvatiSveAsync();
            }

            // Filtriraj po statusu
            if (filterStatus == "Aktivni")
            {
                korisnici = korisnici.Where(k => k.Aktivan);
            }
            else if (filterStatus == "Neaktivni")
            {
                korisnici = korisnici.Where(k => !k.Aktivan);
            }

            var listaStavki = new List<KorisnikListaStavkaViewModel>();

            foreach (var k in korisnici)
            {
                var uloga = await _korisnikRepozitorij.DohvatiRoleKorisnikaAsync(k);
                var brojRezervacija =
                    await _korisnikRepozitorij.DohvatiBrojRezervacijaKorisnikaAsync(k.Id);
                var jeZakljucan = await _korisnikRepozitorij.JeLiKorisnikZakljucanAsync(k.Id);

                listaStavki.Add(
                    new KorisnikListaStavkaViewModel
                    {
                        Id = k.Id,
                        Ime = k.Ime,
                        Prezime = k.Prezime,
                        Email = k.Email ?? string.Empty,
                        Uloga = uloga.FirstOrDefault() ?? "Nema",
                        Aktivan = k.Aktivan,
                        DatumRegistracije = k.DatumRegistracije,
                        JeZakljucan = jeZakljucan,
                        BrojRezervacija = brojRezervacija,
                    }
                );
            }

            var sveUloge = await _korisnikRepozitorij.DohvatiSveRoleAsync();
            var brojAktivnih = await _korisnikRepozitorij.PrebrojAktivneAsync();
            var brojNeaktivnih = await _korisnikRepozitorij.PrebrojNeaktivneAsync();
            var brojZakljucanih = await _korisnikRepozitorij.PrebrojZakljucaneAsync();
            var brojPoUlogama = await _korisnikRepozitorij.DohvatiBrojKorisnikaPoUlogamaAsync();

            return new AdminKorisnikListaViewModel
            {
                Korisnici = listaStavki,
                UkupnoKorisnika = listaStavki.Count,
                FilterUloga = filterUloga,
                FilterStatus = filterStatus,
                DostupneUloge = sveUloge,
                BrojAktivnih = brojAktivnih,
                BrojNeaktivnih = brojNeaktivnih,
                BrojZakljucanih = brojZakljucanih,
                BrojVozaca = brojPoUlogama.GetValueOrDefault("Vozac", 0),
                BrojMenadzera = brojPoUlogama.GetValueOrDefault("Menadzer", 0),
                BrojAdministratora = brojPoUlogama.GetValueOrDefault("Administrator", 0),
            };
        }

        public async Task<AdminKorisnikDetaljiViewModel?> DohvatiAdminDetaljeKorisnikaAsync(
            string id
        )
        {
            var korisnik = await _korisnikRepozitorij.DohvatiPoIdAsync(id);
            if (korisnik == null)
                return null;

            var uloga = await _korisnikRepozitorij.DohvatiRoleKorisnikaAsync(korisnik);
            var jeZakljucan = await _korisnikRepozitorij.JeLiKorisnikZakljucanAsync(id);
            var brojRezervacija = await _korisnikRepozitorij.DohvatiBrojRezervacijaKorisnikaAsync(
                id
            );
            var brojAktivnihRezervacija =
                await _korisnikRepozitorij.DohvatiBrojAktivnihRezervacijaKorisnikaAsync(id);
            var brojNotifikacija = await _korisnikRepozitorij.DohvatiBrojNotifikacijaKorisnikaAsync(
                id
            );
            var brojNecitanihNotifikacija =
                await _korisnikRepozitorij.DohvatiBrojNecitanihNotifikacijaKorisnikaAsync(id);

            string? parkingNaziv = null;
            if (korisnik.MenadzerOdgovorniParkingId.HasValue)
            {
                var parking = await _parkingRepozitorij.DohvatiPoIdAsync(
                    korisnik.MenadzerOdgovorniParkingId.Value
                );
                parkingNaziv = parking?.Naziv;
            }

            return new AdminKorisnikDetaljiViewModel
            {
                Id = korisnik.Id,
                Ime = korisnik.Ime,
                Prezime = korisnik.Prezime,
                Email = korisnik.Email ?? string.Empty,
                Uloga = uloga.FirstOrDefault() ?? "Nema",
                Aktivan = korisnik.Aktivan,
                DatumRegistracije = korisnik.DatumRegistracije,
                JeZakljucan = jeZakljucan,
                BrojVozacke = korisnik.BrojVozacke,
                MenadzerOdgovorniParkingId = korisnik.MenadzerOdgovorniParkingId,
                ParkingNaziv = parkingNaziv,
                BrojRezervacija = brojRezervacija,
                BrojAktivnihRezervacija = brojAktivnihRezervacija,
                BrojNotifikacija = brojNotifikacija,
                BrojNecitanihNotifikacija = brojNecitanihNotifikacija,
            };
        }

        public async Task<AdminKorisnikKreirajViewModel> DohvatiAdminViewModelZaKreiranjeAsync()
        {
            return new AdminKorisnikKreirajViewModel
            {
                DostupneUloge = await _korisnikRepozitorij.DohvatiSveUlogeZaSelectListAsync(),
                DostupniParkinzi = await _korisnikRepozitorij.DohvatiSveParkingeZaSelectListAsync(),
            };
        }

        public async Task<AdminKorisnikUrediViewModel?> DohvatiAdminViewModelZaUredjivanjeAsync(
            string id
        )
        {
            var korisnik = await _korisnikRepozitorij.DohvatiPoIdAsync(id);
            if (korisnik == null)
                return null;

            var uloga = await _korisnikRepozitorij.DohvatiRoleKorisnikaAsync(korisnik);
            var jeZakljucan = await _korisnikRepozitorij.JeLiKorisnikZakljucanAsync(id);

            return new AdminKorisnikUrediViewModel
            {
                Id = korisnik.Id,
                Ime = korisnik.Ime,
                Prezime = korisnik.Prezime,
                Email = korisnik.Email ?? string.Empty,
                Aktivan = korisnik.Aktivan,
                Zakljucan = jeZakljucan,
                Uloga = uloga.FirstOrDefault() ?? "Vozac",
                BrojVozacke = korisnik.BrojVozacke,
                MenadzerOdgovorniParkingId = korisnik.MenadzerOdgovorniParkingId,
                DostupneUloge = await _korisnikRepozitorij.DohvatiSveUlogeZaSelectListAsync(),
                DostupniParkinzi = await _korisnikRepozitorij.DohvatiSveParkingeZaSelectListAsync(),
            };
        }

        public async Task<AdminStatistikaViewModel> DohvatiAdminStatistikuAsync()
        {
            var ukupnoKorisnika = await _korisnikRepozitorij.PrebrojAsync();
            var ukupnoParkinga = await _parkingRepozitorij.PrebrojAsync();
            var ukupnoRezervacija = await _parkingRepozitorij.PrebrojRezervacijeAsync();
            var ukupniPrihod = await _parkingRepozitorij.DohvatiUkupniPrihodAsync();

            var brojPoUlogama = await _korisnikRepozitorij.DohvatiBrojKorisnikaPoUlogamaAsync();
            var brojAktivnih = await _korisnikRepozitorij.PrebrojAktivneAsync();
            var brojNeaktivnih = await _korisnikRepozitorij.PrebrojNeaktivneAsync();
            var brojZakljucanih = await _korisnikRepozitorij.PrebrojZakljucaneAsync();

            var registracijePoDanima = await _korisnikRepozitorij.DohvatiRegistracijePoDanimaAsync(
                30
            );
            var nedavneAktivnosti = await _korisnikRepozitorij.DohvatiNedavneAktivnostiAsync(10);

            return new AdminStatistikaViewModel
            {
                UkupnoKorisnika = ukupnoKorisnika,
                UkupnoParkinga = ukupnoParkinga,
                UkupnoRezervacija = ukupnoRezervacija,
                UkupniPrihod = ukupniPrihod,
                BrojVozaca = brojPoUlogama.GetValueOrDefault("Vozac", 0),
                BrojMenadzera = brojPoUlogama.GetValueOrDefault("Menadzer", 0),
                BrojAdministratora = brojPoUlogama.GetValueOrDefault("Administrator", 0),
                BrojAktivnih = brojAktivnih,
                BrojNeaktivnih = brojNeaktivnih,
                BrojZakljucanih = brojZakljucanih,
                RegistracijePoDanima = registracijePoDanima,
                NedavneAktivnosti = nedavneAktivnosti,
            };
        }

        public async Task<(bool Uspjeh, string[] Greske)> AdminKreirajKorisnikaAsync(
            AdminKorisnikKreirajViewModel model
        )
        {
            if (await EmailVecPostojiAsync(model.Email))
            {
                return (false, new[] { $"Korisnik sa emailom {model.Email} već postoji!" });
            }

            var korisnik = new Korisnik
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                DatumRegistracije = DateTime.Now,
                Aktivan = true,
                BrojVozacke = model.Uloga == "Vozac" ? model.BrojVozacke : null,
                MenadzerOdgovorniParkingId =
                    model.Uloga == "Menadzer" ? model.MenadzerOdgovorniParkingId : null,
            };

            var rezultat = await _korisnikRepozitorij.DodajAsync(korisnik, model.Lozinka);

            if (!rezultat.Succeeded)
            {
                return (false, rezultat.Errors.Select(e => e.Description).ToArray());
            }

            var ulogaRezultat = await _korisnikRepozitorij.DodajUloguKorisnikuAsync(
                korisnik,
                model.Uloga
            );

            if (!ulogaRezultat.Succeeded)
            {
                return (false, ulogaRezultat.Errors.Select(e => e.Description).ToArray());
            }

            return (true, Array.Empty<string>());
        }

        public async Task<(bool Uspjeh, string[] Greske)> AdminAzurirajKorisnikaAsync(
            AdminKorisnikUrediViewModel model
        )
        {
            var korisnik = await _korisnikRepozitorij.DohvatiPoIdAsync(model.Id);
            if (korisnik == null)
            {
                return (false, new[] { "Korisnik nije pronađen!" });
            }

            if (korisnik.Email != model.Email && await EmailVecPostojiAsync(model.Email, model.Id))
            {
                return (false, new[] { $"Korisnik sa emailom {model.Email} već postoji!" });
            }

            // Ažuriranje osnovnih podataka
            korisnik.Ime = model.Ime;
            korisnik.Prezime = model.Prezime;
            korisnik.Email = model.Email;
            korisnik.UserName = model.Email;
            korisnik.Aktivan = model.Aktivan;

            // Ažuriranje specifičnih polja prema ulozi
            if (model.Uloga == "Vozac")
            {
                korisnik.BrojVozacke = model.BrojVozacke;
                korisnik.MenadzerOdgovorniParkingId = null;
            }
            else if (model.Uloga == "Menadzer")
            {
                korisnik.BrojVozacke = null;
                korisnik.MenadzerOdgovorniParkingId = model.MenadzerOdgovorniParkingId;
            }
            else
            {
                korisnik.BrojVozacke = null;
                korisnik.MenadzerOdgovorniParkingId = null;
            }

            // Ažuriranje uloge ako se promijenila
            var trenutneUloge = await _korisnikRepozitorij.DohvatiRoleKorisnikaAsync(korisnik);
            var trenutnaUloga = trenutneUloge.FirstOrDefault();

            if (trenutnaUloga != model.Uloga)
            {
                if (!string.IsNullOrEmpty(trenutnaUloga))
                {
                    await _korisnikRepozitorij.UkloniUloguKorisnikuAsync(korisnik, trenutnaUloga);
                }
                await _korisnikRepozitorij.DodajUloguKorisnikuAsync(korisnik, model.Uloga);
            }

            // Ažuriranje zaključanosti
            if (model.Zakljucan)
            {
                await _korisnikRepozitorij.ZakljucajKorisnikaAsync(model.Id);
            }
            else
            {
                await _korisnikRepozitorij.OtkljucajKorisnikaAsync(model.Id);
            }

            var rezultat = await _korisnikRepozitorij.AzurirajAsync(korisnik);

            if (!rezultat.Succeeded)
            {
                return (false, rezultat.Errors.Select(e => e.Description).ToArray());
            }

            return (true, Array.Empty<string>());
        }

        // ========== MENADŽER RADNJE ==========

        public async Task<MenadzerZaposleniciViewModel> DohvatiMenadzerZaposlenikeAsync(
            string? filter = null
        )
        {
            var parkingId = 1; // Privremeno

            var zaposlenici = await _korisnikRepozitorij.DohvatiZaposlenikePoParkinguAsync(
                parkingId
            );
            var parking = await _parkingRepozitorij.DohvatiPoIdAsync(parkingId);

            var listaStavki = new List<KorisnikListaStavkaViewModel>();

            foreach (var z in zaposlenici)
            {
                var uloga = await _korisnikRepozitorij.DohvatiRoleKorisnikaAsync(z);
                listaStavki.Add(
                    new KorisnikListaStavkaViewModel
                    {
                        Id = z.Id,
                        Ime = z.Ime,
                        Prezime = z.Prezime,
                        Email = z.Email ?? string.Empty,
                        Uloga = uloga.FirstOrDefault() ?? "Menadzer",
                        Aktivan = z.Aktivan,
                        DatumRegistracije = z.DatumRegistracije,
                        JeZakljucan = await _korisnikRepozitorij.JeLiKorisnikZakljucanAsync(z.Id),
                    }
                );
            }

            // Filtriraj ako je potrebno
            if (!string.IsNullOrEmpty(filter))
            {
                listaStavki = listaStavki
                    .Where(z =>
                        z.Ime.Contains(filter, StringComparison.OrdinalIgnoreCase)
                        || z.Prezime.Contains(filter, StringComparison.OrdinalIgnoreCase)
                        || z.Email.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            return new MenadzerZaposleniciViewModel
            {
                ParkingId = parkingId,
                ParkingNaziv = parking?.Naziv ?? "Nepoznat",
                Zaposlenici = listaStavki,
                UkupnoZaposlenih = listaStavki.Count,
                AktivnihZaposlenih = listaStavki.Count(z => z.Aktivan),
                Filter = filter,
            };
        }

        public async Task<MenadzerRadniciViewModel> DohvatiMenadzerRadnikeAsync()
        {
            var parkingId = 1; // Privremeno

            var radnici = await _korisnikRepozitorij.DohvatiZaposlenikePoParkinguAsync(parkingId);
            var brojAktivnihDanas = await _korisnikRepozitorij.DohvatiBrojAktivnihRadnikaDanasAsync(
                parkingId
            );

            var listaStavki = new List<KorisnikListaStavkaViewModel>();

            foreach (var r in radnici)
            {
                var uloga = await _korisnikRepozitorij.DohvatiRoleKorisnikaAsync(r);
                listaStavki.Add(
                    new KorisnikListaStavkaViewModel
                    {
                        Id = r.Id,
                        Ime = r.Ime,
                        Prezime = r.Prezime,
                        Email = r.Email ?? string.Empty,
                        Uloga = uloga.FirstOrDefault() ?? "Menadzer",
                        Aktivan = r.Aktivan,
                        DatumRegistracije = r.DatumRegistracije,
                    }
                );
            }

            return new MenadzerRadniciViewModel
            {
                Radnici = listaStavki,
                UkupnoRadnika = listaStavki.Count,
                BrojAktivnihDanas = brojAktivnihDanas,
            };
        }

        // ========== VOZAČ RADNJE ==========

        public async Task<VozacProfilViewModel?> DohvatiVozacProfilAsync(string korisnikId)
        {
            var korisnik = await _korisnikRepozitorij.DohvatiPoIdAsync(korisnikId);
            if (korisnik == null)
                return null;

            var brojRezervacija = await _korisnikRepozitorij.DohvatiBrojRezervacijaKorisnikaAsync(
                korisnikId
            );
            var brojAktivnihRezervacija =
                await _korisnikRepozitorij.DohvatiBrojAktivnihRezervacijaKorisnikaAsync(korisnikId);
            var brojNotifikacija = await _korisnikRepozitorij.DohvatiBrojNotifikacijaKorisnikaAsync(
                korisnikId
            );
            var brojNecitanihNotifikacija =
                await _korisnikRepozitorij.DohvatiBrojNecitanihNotifikacijaKorisnikaAsync(
                    korisnikId
                );

            // Dohvati posljednje rezervacije
            var posljednjeRezervacije =
                await _parkingRepozitorij.DohvatiPosljednjeRezervacijeKorisnikaAsync(korisnikId, 5);

            return new VozacProfilViewModel
            {
                Id = korisnik.Id,
                Ime = korisnik.Ime,
                Prezime = korisnik.Prezime,
                Email = korisnik.Email ?? string.Empty,
                BrojVozacke = korisnik.BrojVozacke,
                DatumRegistracije = korisnik.DatumRegistracije,
                BrojRezervacija = brojRezervacija,
                BrojAktivnihRezervacija = brojAktivnihRezervacija,
                BrojNotifikacija = brojNotifikacija,
                BrojNecitanihNotifikacija = brojNecitanihNotifikacija,
                PosljednjeRezervacije = posljednjeRezervacije
                    .Select(r => new VozacProfilViewModel.PosljednjaRezervacija
                    {
                        RezervacijaId = r.RezervacijaId,
                        ParkingNaziv = r.Parking?.Naziv ?? "Nepoznat",
                        DatumPocetka = r.PocetakRezervacije,
                        DatumKraja = r.KrajRezervacije,
                        Status = r.StatusRezervacije.ToString(),
                        Cijena = r.UkupnaCijena,
                    })
                    .ToList(),
            };
        }

        // ========== ZAJEDNIČKE RADNJE ==========

        public async Task<string?> DohvatiUloguKorisnikaAsync(string korisnikId)
        {
            var korisnik = await _korisnikRepozitorij.DohvatiPoIdAsync(korisnikId);
            if (korisnik == null)
                return null;

            var uloge = await _korisnikRepozitorij.DohvatiRoleKorisnikaAsync(korisnik);
            return uloge.FirstOrDefault();
        }

        public async Task<(bool Uspjeh, string Greska)> ZakljucajKorisnikaAsync(string id)
        {
            var rezultat = await _korisnikRepozitorij.ZakljucajKorisnikaAsync(id);
            return rezultat.Succeeded
                ? (true, string.Empty)
                : (false, "Greška pri zaključavanju korisnika!");
        }

        public async Task<(bool Uspjeh, string Greska)> OtkljucajKorisnikaAsync(string id)
        {
            var rezultat = await _korisnikRepozitorij.OtkljucajKorisnikaAsync(id);
            return rezultat.Succeeded
                ? (true, string.Empty)
                : (false, "Greška pri otključavanju korisnika!");
        }

        public async Task<(bool Uspjeh, string Greska)> ObrisiKorisnikaAsync(string id)
        {
            var korisnik = await _korisnikRepozitorij.DohvatiPoIdAsync(id);
            if (korisnik == null)
            {
                return (false, "Korisnik nije pronađen!");
            }

            var rezultat = await _korisnikRepozitorij.ObrisiAsync(korisnik);

            if (!rezultat.Succeeded)
            {
                return (
                    false,
                    rezultat.Errors.FirstOrDefault()?.Description ?? "Greška pri brisanju!"
                );
            }

            return (true, string.Empty);
        }

        // ========== ZA DROPDOWN LISTE ==========

        public async Task<IEnumerable<SelectListItem>> DohvatiSveUlogeZaSelectListAsync()
        {
            return await _korisnikRepozitorij.DohvatiSveUlogeZaSelectListAsync();
        }

        public async Task<IEnumerable<SelectListItem>> DohvatiSveParkingeZaSelectListAsync()
        {
            return await _korisnikRepozitorij.DohvatiSveParkingeZaSelectListAsync();
        }

        // ========== PROVJERE ==========

        public async Task<bool> EmailVecPostojiAsync(string email, string? izuzmiId = null)
        {
            return await _korisnikRepozitorij.PostojiLiSaEmailomAsync(email, izuzmiId);
        }
    }
}
