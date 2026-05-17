using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Parking.Admin;
using smartPark.Models.ViewModels.Parking.Menadzer;
using smartPark.Models.ViewModels.Parking.Shared;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class ParkingService : IParkingService
    {
        private readonly IParkingRepository _parkingRepository;
        private readonly IKorisnikRepository _korisnikRepository;

        public ParkingService(
            IParkingRepository parkingRepository,
            IKorisnikRepository korisnikRepository
        )
        {
            _parkingRepository = parkingRepository;
            _korisnikRepository = korisnikRepository;
        }

        // ========== OSNOVNE RADNJE ==========

        public async Task<Parking?> DohvatiParkingPoIdAsync(int id)
        {
            return await _parkingRepository.DohvatiPoIdAsync(id);
        }

        public async Task<IEnumerable<Parking>> DohvatiSveParkingeAsync()
        {
            return await _parkingRepository.DohvatiSveAsync();
        }

        public async Task<IEnumerable<Parking>> DohvatiAktivneParkingeAsync()
        {
            return await _parkingRepository.DohvatiAktivneAsync();
        }

        // ========== ADMIN RADNJE ==========

        public async Task<AdminParkingListaViewModel> DohvatiAdminListuParkingaAsync(
            string? filterStatus = null,
            string? filterTip = null
        )
        {
            var parkinzi = await _parkingRepository.DohvatiSveAsync();
            var lista = parkinzi.ToList();

            // Filtriranje
            if (filterStatus == "Aktivni")
            {
                lista = lista.Where(p => p.Aktivan).ToList();
            }
            else if (filterStatus == "Neaktivni")
            {
                lista = lista.Where(p => !p.Aktivan).ToList();
            }

            if (filterTip == "Otvoreni")
            {
                lista = lista.Where(p => p.TipParkinga == TipParkinga.Otvoreni).ToList();
            }
            else if (filterTip == "Zatvoreni")
            {
                lista = lista.Where(p => p.TipParkinga == TipParkinga.Zatvoreni).ToList();
            }

            // Statistika
            var ukupnoMjesta = lista.Sum(p => p.UkupnoMjesta);
            var ukupnoSlobodnih = lista.Sum(p => p.SlobodnaMjesta);
            var ukupniPrihodDanas = await _parkingRepository.DohvatiUkupniPrihodZaPeriodAsync(
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );

            return new AdminParkingListaViewModel
            {
                Parkinzi = lista
                    .Select(p => new ParkingListaStavkaViewModel
                    {
                        ParkingId = p.ParkingId,
                        Naziv = p.Naziv,
                        Adresa = p.Adresa,
                        UkupnoMjesta = p.UkupnoMjesta,
                        SlobodnaMjesta = p.SlobodnaMjesta,
                        CijenaPoSatu = p.CijenaPoSatu,
                        TipParkinga = p.TipParkinga,
                        Aktivan = p.Aktivan,
                        MenadzerIme = p.Menadzer?.Ime,
                        MenadzerPrezime = p.Menadzer?.Prezime,
                    })
                    .ToList(),
                UkupnoParkinga = lista.Count,
                AktivnihParkinga = lista.Count(p => p.Aktivan),
                NeaktivnihParkinga = lista.Count(p => !p.Aktivan),
                UkupnoMjesta = ukupnoMjesta,
                UkupnoSlobodnihMjesta = ukupnoSlobodnih,
                UkupniDnevniPrihod = ukupniPrihodDanas,
                DostupniTipovi = new[] { TipParkinga.Otvoreni, TipParkinga.Zatvoreni },
            };
        }

        public async Task<AdminParkingDetaljiViewModel?> DohvatiAdminDetaljeParkingaAsync(int id)
        {
            var parking = await _parkingRepository.DohvatiPoIdSaRezervacijamaAsync(id);
            if (parking == null)
                return null;

            // Statistika za parking
            var prihodDanas = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                id,
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var prihodSedmica = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                id,
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var prihodMjesec = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                id,
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );

            var brojRezDanas = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                id,
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var brojRezSedmica = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                id,
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var brojRezMjesec = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                id,
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );

            return new AdminParkingDetaljiViewModel
            {
                ParkingId = parking.ParkingId,
                Naziv = parking.Naziv,
                Adresa = parking.Adresa,
                Latitude = parking.Latitude,
                Longitude = parking.Longitude,
                UkupnoMjesta = parking.UkupnoMjesta,
                SlobodnaMjesta = parking.SlobodnaMjesta,
                CijenaPoSatu = parking.CijenaPoSatu,
                TipParkinga = parking.TipParkinga,
                Aktivan = parking.Aktivan,
                DatumKreiranja = parking.DatumKreiranja,
                MenadzerIme = parking.Menadzer?.Ime,
                MenadzerPrezime = parking.Menadzer?.Prezime,
                MenadzerEmail = parking.Menadzer?.Email ?? string.Empty,
                PrihodDanas = prihodDanas,
                PrihodSedmica = prihodSedmica,
                PrihodMjesec = prihodMjesec,
                BrojRezervacijaDanas = brojRezDanas,
                BrojRezervacijaSedmica = brojRezSedmica,
                BrojRezervacijaMjesec = brojRezMjesec,
            };
        }

        public async Task<AdminParkingKreirajViewModel> DohvatiAdminViewModelZaKreiranjeAsync()
        {
            return new AdminParkingKreirajViewModel
            {
                DostupniMenadzeri = await _parkingRepository.DohvatiSveMenadzereZaSelectListAsync(),
                TipParkinga = TipParkinga.Otvoreni,
                AktivanOdmah = true,
            };
        }

        public async Task<AdminParkingUrediViewModel?> DohvatiAdminViewModelZaUredjivanjeAsync(
            int id
        )
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(id);
            if (parking == null)
                return null;

            return new AdminParkingUrediViewModel
            {
                ParkingId = parking.ParkingId,
                Naziv = parking.Naziv,
                Adresa = parking.Adresa,
                Latitude = parking.Latitude,
                Longitude = parking.Longitude,
                UkupnoMjesta = parking.UkupnoMjesta,
                SlobodnaMjesta = parking.SlobodnaMjesta,
                CijenaPoSatu = parking.CijenaPoSatu,
                TipParkinga = parking.TipParkinga,
                Aktivan = parking.Aktivan,
                MenadzerId = parking.MenadzerID,
                DostupniMenadzeri = await _parkingRepository.DohvatiSveMenadzereZaSelectListAsync(),
            };
        }

        public async Task<AdminParkingStatistikaViewModel> DohvatiAdminStatistikuParkingaAsync()
        {
            var ukupnoParkinga = await _parkingRepository.DohvatiUkupnoParkingaAsync();
            var aktivnihParkinga = await _parkingRepository.DohvatiBrojAktivnihParkingaAsync();
            var ukupnoMjesta = await _parkingRepository.DohvatiUkupnoMjestaAsync();
            var ukupnoSlobodnih = await _parkingRepository.DohvatiUkupnoSlobodnihMjestaAsync();

            var prihodDanas = await _parkingRepository.DohvatiUkupniPrihodZaPeriodAsync(
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var prihodSedmica = await _parkingRepository.DohvatiUkupniPrihodZaPeriodAsync(
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var prihodMjesec = await _parkingRepository.DohvatiUkupniPrihodZaPeriodAsync(
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );
            var prihodGodina = await _parkingRepository.DohvatiUkupniPrihodZaPeriodAsync(
                DateTime.Now.AddDays(-365),
                DateTime.Now
            );

            var brojPoTipu = await _parkingRepository.DohvatiBrojParkingaPoTipuAsync();
            var prosjecnaCijenaPoTipu =
                await _parkingRepository.DohvatiProsjecnuCijenuPoTipuAsync();
            var najpopularniji = await _parkingRepository.DohvatiNajpopularnijeParkingeAsync(5);
            var rezervacijePoDanima = await _parkingRepository.DohvatiRezervacijePoDanimaAsync(
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );
            var prihodiPoDanima = await _parkingRepository.DohvatiPrihodePoDanimaAsync(
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );

            return new AdminParkingStatistikaViewModel
            {
                UkupnoParkinga = ukupnoParkinga,
                AktivnihParkinga = aktivnihParkinga,
                NeaktivnihParkinga = ukupnoParkinga - aktivnihParkinga,
                UkupnoMjesta = ukupnoMjesta,
                UkupnoSlobodnihMjesta = ukupnoSlobodnih,
                UkupniPrihodDanas = prihodDanas,
                UkupniPrihodSedmica = prihodSedmica,
                UkupniPrihodMjesec = prihodMjesec,
                UkupniPrihodGodina = prihodGodina,
                BrojOtvorenih = brojPoTipu.GetValueOrDefault(TipParkinga.Otvoreni, 0),
                BrojZatvorenih = brojPoTipu.GetValueOrDefault(TipParkinga.Zatvoreni, 0),
                ProsjecnaCijenaOtvorenih = prosjecnaCijenaPoTipu.GetValueOrDefault(
                    TipParkinga.Otvoreni,
                    0
                ),
                ProsjecnaCijenaZatvorenih = prosjecnaCijenaPoTipu.GetValueOrDefault(
                    TipParkinga.Zatvoreni,
                    0
                ),
                NajpopularnijiParkinzi = najpopularniji,
                RezervacijePoDanima = rezervacijePoDanima,
                PrihodiPoDanima = prihodiPoDanima,
            };
        }

        public async Task<Parking> AdminKreirajParkingAsync(AdminParkingKreirajViewModel model)
        {
            var parking = new Parking
            {
                Naziv = model.Naziv,
                Adresa = model.Adresa,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                UkupnoMjesta = model.UkupnoMjesta,
                SlobodnaMjesta = model.UkupnoMjesta, // Na početku su sva mjesta slobodna
                CijenaPoSatu = model.CijenaPoSatu,
                TipParkinga = model.TipParkinga,
                Aktivan = model.AktivanOdmah,
                DatumKreiranja = DateTime.Now,
                MenadzerID = string.IsNullOrEmpty(model.MenadzerId) ? null : model.MenadzerId,
            };

            await _parkingRepository.DodajAsync(parking);
            await _parkingRepository.SacuvajPromjeneAsync();

            return parking;
        }

        public async Task<Parking?> AdminAzurirajParkingAsync(AdminParkingUrediViewModel model)
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(model.ParkingId);
            if (parking == null)
                return null;

            parking.Naziv = model.Naziv;
            parking.Adresa = model.Adresa;
            parking.Latitude = model.Latitude;
            parking.Longitude = model.Longitude;
            parking.UkupnoMjesta = model.UkupnoMjesta;
            parking.SlobodnaMjesta = model.SlobodnaMjesta;
            parking.CijenaPoSatu = model.CijenaPoSatu;
            parking.TipParkinga = model.TipParkinga;
            parking.Aktivan = model.Aktivan;
            parking.MenadzerID = string.IsNullOrEmpty(model.MenadzerId) ? null : model.MenadzerId;

            _parkingRepository.Izmijeni(parking);
            await _parkingRepository.SacuvajPromjeneAsync();

            return parking;
        }

        public async Task<bool> AdminObrisiParkingAsync(int id)
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(id);
            if (parking == null)
                return false;

            _parkingRepository.Obrisi(parking);
            await _parkingRepository.SacuvajPromjeneAsync();

            return true;
        }

        // ========== MENADŽER RADNJE ==========

        public async Task<MenadzerParkingDetaljiViewModel?> DohvatiMenadzerParkingDetaljiAsync(
            string menadzerId
        )
        {
            var parking = await _parkingRepository.DohvatiParkingPoMenadzeruAsync(menadzerId);
            if (parking == null)
                return null;

            var aktivneRezervacije =
                await _parkingRepository.DohvatiAktivneRezervacijeZaParkingAsync(parking.ParkingId);
            var brojAktivnihTrenutno =
                await _parkingRepository.DohvatiBrojAktivnihRezervacijaTrenutnoAsync(
                    parking.ParkingId
                );
            var prihodDanas = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var prihodSedmica = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var brojRezDanas = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var brojRezSedmica = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );

            return new MenadzerParkingDetaljiViewModel
            {
                ParkingId = parking.ParkingId,
                Naziv = parking.Naziv,
                Adresa = parking.Adresa,
                UkupnoMjesta = parking.UkupnoMjesta,
                SlobodnaMjesta = parking.SlobodnaMjesta,
                CijenaPoSatu = parking.CijenaPoSatu,
                TipParkinga = parking.TipParkinga,
                Aktivan = parking.Aktivan,
                DatumKreiranja = parking.DatumKreiranja,
                BrojRezervacijaDanas = brojRezDanas,
                BrojRezervacijaSedmica = brojRezSedmica,
                PrihodDanas = prihodDanas,
                PrihodSedmica = prihodSedmica,
                BrojAktivnihRezervacijaTrenutno = brojAktivnihTrenutno,
                AktivneRezervacije = aktivneRezervacije,
            };
        }

        public async Task<MenadzerParkingStatistikaViewModel?> DohvatiMenadzerStatistikuParkingaAsync(
            string menadzerId
        )
        {
            var parking = await _parkingRepository.DohvatiParkingPoMenadzeruAsync(menadzerId);
            if (parking == null)
                return null;

            var rezervacijePoSatima = await _parkingRepository.DohvatiRezervacijePoSatimaAsync(
                parking.ParkingId
            );
            var rezervacijePoDanima =
                await _parkingRepository.DohvatiRezervacijePoDanimaSedmiceAsync(parking.ParkingId);
            var rezervacijeZadnjih7Dana = await _parkingRepository.DohvatiRezervacijePoDanimaAsync(
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var prihodiZadnjih7Dana = await _parkingRepository.DohvatiPrihodePoDanimaAsync(
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );

            var prihodDanas = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var prihodSedmica = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var prihodMjesec = await _parkingRepository.DohvatiPrihodZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );

            var brojRezDanas = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var brojRezSedmica = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var brojRezMjesec = await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );

            var zauzetostDanas = await _parkingRepository.DohvatiProsjecnuZauzetostAsync(
                parking.ParkingId,
                DateTime.Now.Date,
                DateTime.Now.Date.AddDays(1)
            );
            var zauzetostSedmica = await _parkingRepository.DohvatiProsjecnuZauzetostAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var zauzetostMjesec = await _parkingRepository.DohvatiProsjecnuZauzetostAsync(
                parking.ParkingId,
                DateTime.Now.AddDays(-30),
                DateTime.Now
            );

            return new MenadzerParkingStatistikaViewModel
            {
                ParkingId = parking.ParkingId,
                ParkingNaziv = parking.Naziv,
                RezervacijaDanas = brojRezDanas,
                RezervacijaSedmica = brojRezSedmica,
                RezervacijaMjesec = brojRezMjesec,
                PrihodDanas = prihodDanas,
                PrihodSedmica = prihodSedmica,
                PrihodMjesec = prihodMjesec,
                ProsjecnaZauzetostDanas = zauzetostDanas,
                ProsjecnaZauzetostSedmica = zauzetostSedmica,
                ProsjecnaZauzetostMjesec = zauzetostMjesec,
                RezervacijePoSatima = rezervacijePoSatima,
                RezervacijePoDanimaSedmice = rezervacijePoDanima,
                RezervacijeZadnjih7Dana = rezervacijeZadnjih7Dana,
                PrihodiZadnjih7Dana = prihodiZadnjih7Dana,
            };
        }

        public async Task<MenadzerParkingUrediViewModel?> DohvatiMenadzerViewModelZaUredjivanjeAsync(
            string menadzerId
        )
        {
            var parking = await _parkingRepository.DohvatiParkingPoMenadzeruAsync(menadzerId);
            if (parking == null)
                return null;

            return new MenadzerParkingUrediViewModel
            {
                ParkingId = parking.ParkingId,
                Naziv = parking.Naziv,
                Adresa = parking.Adresa,
                SlobodnaMjesta = parking.SlobodnaMjesta,
                CijenaPoSatu = parking.CijenaPoSatu,
                Aktivan = parking.Aktivan,
            };
        }

        public async Task<Parking?> MenadzerAzurirajParkingAsync(
            MenadzerParkingUrediViewModel model
        )
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(model.ParkingId);
            if (parking == null)
                return null;

            // Menadžer može mijenjati samo slobodna mjesta, cijenu i status
            parking.SlobodnaMjesta = model.SlobodnaMjesta;
            parking.CijenaPoSatu = model.CijenaPoSatu;
            parking.Aktivan = model.Aktivan;

            _parkingRepository.Izmijeni(parking);
            await _parkingRepository.SacuvajPromjeneAsync();

            return parking;
        }

        // ========== ZAJEDNIČKE RADNJE ==========

        public async Task<bool> ParkingPostojiAsync(int id)
        {
            return await _parkingRepository.PostojiLiAsync(id);
        }

        public async Task<bool> NazivParkingaPostojiAsync(string naziv, int? izuzmiId = null)
        {
            return await _parkingRepository.PostojiLiNazivAsync(naziv, izuzmiId);
        }

        public async Task<int> DohvatiBrojSlobodnihMjestaAsync(int parkingId)
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(parkingId);
            return parking?.SlobodnaMjesta ?? 0;
        }

        public async Task<decimal> IzracunajCijenuAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj
        )
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(parkingId);
            if (parking == null)
                return 0;

            var sati = (decimal)(kraj - pocetak).TotalHours;
            return sati * parking.CijenaPoSatu;
        }

        // ========== ZA DROPDOWN LISTE ==========

        public async Task<IEnumerable<SelectListItem>> DohvatiSveMenadzereZaSelectListAsync()
        {
            return await _parkingRepository.DohvatiSveMenadzereZaSelectListAsync();
        }
    }
}
