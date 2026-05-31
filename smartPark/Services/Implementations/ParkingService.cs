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
        private readonly IParkingMjestoRepository _parkingMjestoRepository;
        private readonly ICjenovnikRepository _cjenovnikRepository;

        public ParkingService(
            IParkingRepository parkingRepository,
            IKorisnikRepository korisnikRepository,
            IParkingMjestoRepository parkingMjestoRepository,
            ICjenovnikRepository cjenovnikRepository
        )
        {
            _parkingRepository = parkingRepository;
            _korisnikRepository = korisnikRepository;
            _parkingMjestoRepository = parkingMjestoRepository;
            _cjenovnikRepository = cjenovnikRepository;
        }

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
            var cjenovnici = await _cjenovnikRepository.DohvatiSveCjenovnikeAsync();
            var aktivniCjenovnici = cjenovnici.Where(c => c.Aktivan).ToList();

            var list = new[] { new SelectListItem { Value = "", Text = "Bez cjenovnika" } }
                .Concat(aktivniCjenovnici.Select(c => new SelectListItem
                {
                    Value = c.CjenovnikId.ToString(),
                    Text = $"{c.Naziv} (Dnevna: {c.CijenaDnevna} KM/h, Noćna: {c.CijenaNocna} KM/h)"
                }));

            return new AdminParkingKreirajViewModel
            {
                DostupniMenadzeri = await _parkingRepository.DohvatiSveMenadzereZaSelectListAsync(),
                DostupniCjenovniciDefault = list,
                DostupniCjenovniciDan = list,
                DostupniCjenovniciNoc = list,
                TipParkinga = TipParkinga.Otvoreni,
                Aktivan = true,
            };
        }

        public async Task<AdminParkingUrediViewModel?> DohvatiAdminViewModelZaUredjivanjeAsync(
            int id
        )
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(id);
            if (parking == null)
                return null;

            var cjenovnici = await _cjenovnikRepository.DohvatiSveCjenovnikeAsync();
            var aktivniCjenovnici = cjenovnici.Where(c => c.Aktivan && c.ParkingId == id).ToList();

            var list = new[] { new SelectListItem { Value = "", Text = "Bez cjenovnika" } }
                .Concat(aktivniCjenovnici.Select(c => new SelectListItem
                {
                    Value = c.CjenovnikId.ToString(),
                    Text = $"{c.Naziv} (Dnevna: {c.CijenaDnevna} KM/h, Noćna: {c.CijenaNocna} KM/h)"
                }));

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
                MenadzerId = !string.IsNullOrEmpty(parking.MenadzerID) ? parking.MenadzerID.Split(',')[0] : null,
                DefaultniCjenovnikId = parking.DefaultniCjenovnikId,
                DnevniCjenovnikId = parking.DnevniCjenovnikId,
                NocniCjenovnikId = parking.NocniCjenovnikId,
                DostupniCjenovniciDefault = list,
                DostupniCjenovniciDan = list,
                DostupniCjenovniciNoc = list,
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
                Aktivan = model.Aktivan,
                DatumKreiranja = DateTime.Now,
                MenadzerID = model.MenadzerId,
                RadnoVrijeme = model.RadnoVrijeme,
                DefaultniCjenovnikId = model.DefaultniCjenovnikId,
                DnevniCjenovnikId = model.DnevniCjenovnikId,
                NocniCjenovnikId = model.NocniCjenovnikId,
            };

            await _parkingRepository.DodajAsync(parking);
            await _parkingRepository.SacuvajPromjeneAsync();

            // Automatski kreiraj parking mjesta od 1 do UkupnoMjesta
            for (int i = 1; i <= parking.UkupnoMjesta; i++)
            {
                var mjesto = new ParkingMjesto
                {
                    ParkingId = parking.ParkingId,
                    BrojMjesta = i,
                    StatusMjesta = StatusMjesta.Slobodno
                };
                await _parkingMjestoRepository.DodajAsync(mjesto);
            }
            await _parkingMjestoRepository.SacuvajPromjeneAsync();

            return parking;
        }

        public async Task<Parking?> AdminAzurirajParkingAsync(AdminParkingUrediViewModel model)
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(model.ParkingId);
            if (parking == null)
                return null;

            var staroKapacitet = parking.UkupnoMjesta;
            var noviKapacitet = model.UkupnoMjesta;

            parking.Naziv = model.Naziv;
            parking.Adresa = model.Adresa;
            parking.Latitude = model.Latitude;
            parking.Longitude = model.Longitude;
            parking.UkupnoMjesta = model.UkupnoMjesta;
            parking.SlobodnaMjesta = model.SlobodnaMjesta;
            parking.CijenaPoSatu = model.CijenaPoSatu;
            parking.TipParkinga = model.TipParkinga;
            parking.Aktivan = model.Aktivan;
            parking.MenadzerID = model.MenadzerId;
            parking.RadnoVrijeme = model.RadnoVrijeme;
            parking.DefaultniCjenovnikId = model.DefaultniCjenovnikId;
            parking.DnevniCjenovnikId = model.DnevniCjenovnikId;
            parking.NocniCjenovnikId = model.NocniCjenovnikId;

            _parkingRepository.Izmijeni(parking);
            await _parkingRepository.SacuvajPromjeneAsync();

            // Ako je kapacitet promijenjen, prilagodi parking mjesta
            if (noviKapacitet != staroKapacitet)
            {
                var postojecaMjesta = (await _parkingMjestoRepository.DohvatiPoParkinguAsync(parking.ParkingId)).ToList();
                if (noviKapacitet > staroKapacitet)
                {
                    // Dodaj nova mjesta
                    for (int i = staroKapacitet + 1; i <= noviKapacitet; i++)
                    {
                        if (!postojecaMjesta.Any(m => m.BrojMjesta == i))
                        {
                            var mjesto = new ParkingMjesto
                            {
                                ParkingId = parking.ParkingId,
                                BrojMjesta = i,
                                StatusMjesta = StatusMjesta.Slobodno
                            };
                            await _parkingMjestoRepository.DodajAsync(mjesto);
                        }
                    }
                }
                else
                {
                    // Smanji kapacitet: obriši višak mjesta od najvećeg broja unazad
                    var visakMjesta = postojecaMjesta
                        .Where(m => m.BrojMjesta > noviKapacitet)
                        .OrderByDescending(m => m.BrojMjesta)
                        .ToList();

                    foreach (var mjesto in visakMjesta)
                    {
                        _parkingMjestoRepository.Obrisi(mjesto);
                    }
                }
                await _parkingMjestoRepository.SacuvajPromjeneAsync();
            }

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
            var parkinzi = await _parkingRepository.DohvatiSveParkingePoMenadzeruAsync(menadzerId);
            if (parkinzi == null || !parkinzi.Any())
                return null;

            var primaryParking = parkinzi.First();

            int ukupnoMjestaSuma = 0;
            int brojRezDanasSuma = 0;
            int brojRezSedmicaSuma = 0;
            int brojRezMjesecSuma = 0;
            decimal prihodDanasSuma = 0;
            decimal prihodSedmicaSuma = 0;
            decimal prihodMjesecSuma = 0;
            double zauzetostDanasSuma = 0;
            double zauzetostSedmicaSuma = 0;
            double zauzetostMjesecSuma = 0;

            foreach (var p in parkinzi)
            {
                ukupnoMjestaSuma += p.UkupnoMjesta;

                prihodDanasSuma += await _parkingRepository.DohvatiPrihodZaParkingAsync(
                    p.ParkingId,
                    DateTime.Now.Date,
                    DateTime.Now.Date.AddDays(1)
                );
                prihodSedmicaSuma += await _parkingRepository.DohvatiPrihodZaParkingAsync(
                    p.ParkingId,
                    DateTime.Now.AddDays(-7),
                    DateTime.Now
                );
                prihodMjesecSuma += await _parkingRepository.DohvatiPrihodZaParkingAsync(
                    p.ParkingId,
                    DateTime.Now.AddDays(-30),
                    DateTime.Now
                );

                brojRezDanasSuma += await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                    p.ParkingId,
                    DateTime.Now.Date,
                    DateTime.Now.Date.AddDays(1)
                );
                brojRezSedmicaSuma += await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                    p.ParkingId,
                    DateTime.Now.AddDays(-7),
                    DateTime.Now
                );
                brojRezMjesecSuma += await _parkingRepository.DohvatiBrojRezervacijaZaParkingAsync(
                    p.ParkingId,
                    DateTime.Now.AddDays(-30),
                    DateTime.Now
                );

                zauzetostDanasSuma += await _parkingRepository.DohvatiProsjecnuZauzetostAsync(
                    p.ParkingId,
                    DateTime.Now.Date,
                    DateTime.Now.Date.AddDays(1)
                );
                zauzetostSedmicaSuma += await _parkingRepository.DohvatiProsjecnuZauzetostAsync(
                    p.ParkingId,
                    DateTime.Now.AddDays(-7),
                    DateTime.Now
                );
                zauzetostMjesecSuma += await _parkingRepository.DohvatiProsjecnuZauzetostAsync(
                    p.ParkingId,
                    DateTime.Now.AddDays(-30),
                    DateTime.Now
                );
            }

            double prosjecnaZauzetostDanas = parkinzi.Count > 0 ? zauzetostDanasSuma / parkinzi.Count : 0;
            double prosjecnaZauzetostSedmica = parkinzi.Count > 0 ? zauzetostSedmicaSuma / parkinzi.Count : 0;
            double prosjecnaZauzetostMjesec = parkinzi.Count > 0 ? zauzetostMjesecSuma / parkinzi.Count : 0;

            var rezervacijePoSatima = await _parkingRepository.DohvatiRezervacijePoSatimaAsync(
                primaryParking.ParkingId
            );
            var rezervacijePoDanima =
                await _parkingRepository.DohvatiRezervacijePoDanimaSedmiceAsync(primaryParking.ParkingId);
            var rezervacijeZadnjih7Dana = await _parkingRepository.DohvatiRezervacijePoDanimaAsync(
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );
            var prihodiZadnjih7Dana = await _parkingRepository.DohvatiPrihodePoDanimaAsync(
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );

            return new MenadzerParkingStatistikaViewModel
            {
                ParkingId = primaryParking.ParkingId,
                ParkingNaziv = primaryParking.Naziv,
                UkupnoParkinga = parkinzi.Count,
                UkupnoMjesta = ukupnoMjestaSuma,
                RezervacijaDanas = brojRezDanasSuma,
                RezervacijaSedmica = brojRezSedmicaSuma,
                RezervacijaMjesec = brojRezMjesecSuma,
                PrihodDanas = prihodDanasSuma,
                PrihodSedmica = prihodSedmicaSuma,
                PrihodMjesec = prihodMjesecSuma,
                ProsjecnaZauzetostDanas = prosjecnaZauzetostDanas,
                ProsjecnaZauzetostSedmica = prosjecnaZauzetostSedmica,
                ProsjecnaZauzetostMjesec = prosjecnaZauzetostMjesec,
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

        public async Task<bool> DaLiMenadzerUpravljaParkingomAsync(string menadzerId, int parkingId)
        {
            return await _parkingRepository.DaLiMenadzerUpravljaParkingomAsync(menadzerId, parkingId);
        }

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

            decimal dnevnaCijena = 1.50m;
            decimal nocnaCijena = 1.05m;

            if (parking.DefaultniCjenovnikId.HasValue)
            {
                var defaultCjenovnik = await _cjenovnikRepository.DohvatiPoIdCjenovnikAsync(parking.DefaultniCjenovnikId.Value);
                if (defaultCjenovnik != null)
                {
                    dnevnaCijena = defaultCjenovnik.CijenaDnevna;
                    nocnaCijena = defaultCjenovnik.CijenaNocna;
                }
            }

            if (parking.DnevniCjenovnikId.HasValue)
            {
                var dnevniCjenovnik = await _cjenovnikRepository.DohvatiPoIdCjenovnikAsync(parking.DnevniCjenovnikId.Value);
                if (dnevniCjenovnik != null)
                {
                    dnevnaCijena = dnevniCjenovnik.CijenaDnevna;
                }
            }

            if (parking.NocniCjenovnikId.HasValue)
            {
                var nocniCjenovnik = await _cjenovnikRepository.DohvatiPoIdCjenovnikAsync(parking.NocniCjenovnikId.Value);
                if (nocniCjenovnik != null)
                {
                    nocnaCijena = nocniCjenovnik.CijenaNocna;
                }
            }

            var totalHours = (int)Math.Ceiling((kraj - pocetak).TotalHours);
            decimal ukupno = 0;
            for (int i = 0; i < totalHours; i++)
            {
                var hourStart = pocetak.AddHours(i).Hour;
                if (hourStart >= 6 && hourStart < 22)
                {
                    ukupno += dnevnaCijena;
                }
                else
                {
                    ukupno += nocnaCijena;
                }
            }

            return ukupno;
        }

        public async Task<IEnumerable<SelectListItem>> DohvatiSveMenadzereZaSelectListAsync()
        {
            return await _parkingRepository.DohvatiSveMenadzereZaSelectListAsync();
        }

        public async Task PopuniCjenovnikeZaKreirajAsync(AdminParkingKreirajViewModel model)
        {
            var cjenovnici = await _cjenovnikRepository.DohvatiSveCjenovnikeAsync();
            var aktivniCjenovnici = cjenovnici.Where(c => c.Aktivan).ToList();

            var list = new[] { new SelectListItem { Value = "", Text = "Bez cjenovnika" } }
                .Concat(aktivniCjenovnici.Select(c => new SelectListItem
                {
                    Value = c.CjenovnikId.ToString(),
                    Text = $"{c.Naziv} (Dnevna: {c.CijenaDnevna} KM/h, Noćna: {c.CijenaNocna} KM/h)"
                }));

            model.DostupniCjenovniciDefault = list;
            model.DostupniCjenovniciDan = list;
            model.DostupniCjenovniciNoc = list;
        }

        public async Task PopuniCjenovnikeZaUrediAsync(AdminParkingUrediViewModel model)
        {
            var cjenovnici = await _cjenovnikRepository.DohvatiSveCjenovnikeAsync();
            var aktivniCjenovnici = cjenovnici.Where(c => c.Aktivan && c.ParkingId == model.ParkingId).ToList();

            var list = new[] { new SelectListItem { Value = "", Text = "Bez cjenovnika" } }
                .Concat(aktivniCjenovnici.Select(c => new SelectListItem
                {
                    Value = c.CjenovnikId.ToString(),
                    Text = $"{c.Naziv} (Dnevna: {c.CijenaDnevna} KM/h, Noćna: {c.CijenaNocna} KM/h)"
                }));

            model.DostupniCjenovniciDefault = list;
            model.DostupniCjenovniciDan = list;
            model.DostupniCjenovniciNoc = list;
        }
    }
}
