using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Izvjestaj;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class IzvjestajService : IIzvjestajService
    {
        private readonly IIzvjestajRepository _izvjestajRepository;
        private readonly IParkingRepository _parkingRepository;
        private readonly IRezervacijaRepository _rezervacijaRepository;

        public IzvjestajService(
            IIzvjestajRepository izvjestajRepository,
            IParkingRepository parkingRepository,
            IRezervacijaRepository rezervacijaRepository
        )
        {
            _izvjestajRepository = izvjestajRepository;
            _parkingRepository = parkingRepository;
            _rezervacijaRepository = rezervacijaRepository;
        }

        public async Task<Izvjestaj?> DohvatiIzvjestajPoIdAsync(int id)
        {
            return await _izvjestajRepository.DohvatiPoIdAsync(id);
        }

        public async Task<IEnumerable<Izvjestaj>> DohvatiSveIzvjestajeAsync()
        {
            return await _izvjestajRepository.DohvatiSveAsync();
        }

        public async Task<Izvjestaj> GenerisiIzvjestajAsync(IzvjestajKreirajViewModel model)
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(model.ParkingId);
            if (parking == null)
                throw new KeyNotFoundException($"Parking sa ID {model.ParkingId} nije pronađen");

            var ukupnoRezervacija = await _izvjestajRepository.DohvatiBrojRezervacijaZaPeriodAsync(
                model.ParkingId,
                model.PeriodOd,
                model.PeriodDo
            );

            var ukupniPrihod = await _izvjestajRepository.DohvatiPrihodZaPeriodAsync(
                model.ParkingId,
                model.PeriodOd,
                model.PeriodDo
            );

            var izvjestaj = new Izvjestaj
            {
                ParkingId = model.ParkingId,
                TipIzvjestaja = model.TipIzvjestaja,
                PeriodOd = model.PeriodOd,
                PeriodDo = model.PeriodDo,
                UkupnoRezervacija = ukupnoRezervacija,
                UkupniPrihod = ukupniPrihod,
                DatumGenerisanja = DateTime.Now,
            };

            if (model.SacuvajUzBazi)
            {
                await _izvjestajRepository.DodajAsync(izvjestaj);
                await _izvjestajRepository.SacuvajPromjeneAsync();
            }

            return izvjestaj;
        }

        public async Task<PopunjenostIzvjestajViewModel> GenerisiPopunjenostIzvjestajAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            string parkingNaziv;
            int ukupnoMjesta;

            if (parkingId == 0)
            {
                parkingNaziv = "Svi parking prostori";
                var svi = await _parkingRepository.DohvatiSveAsync();
                ukupnoMjesta = svi.Sum(p => p.UkupnoMjesta);
            }
            else
            {
                var parking = await _parkingRepository.DohvatiPoIdAsync(parkingId);
                if (parking == null)
                    throw new KeyNotFoundException($"Parking sa ID {parkingId} nije pronađen");
                parkingNaziv = parking.Naziv;
                ukupnoMjesta = parking.UkupnoMjesta;
            }

            var popunjenostPoDanima = await _izvjestajRepository.DohvatiPopunjenostPoDanimaAsync(
                parkingId,
                od,
                doo
            );
            var prosjecna = await _izvjestajRepository.DohvatiProsjecnuPopunjenostAsync(
                parkingId,
                od,
                doo
            );
            var maksimalna = await _izvjestajRepository.DohvatiMaksimalnuPopunjenostAsync(
                parkingId,
                od,
                doo
            );
            var minimalna = await _izvjestajRepository.DohvatiMinimalnuPopunjenostAsync(
                parkingId,
                od,
                doo
            );

            var dnevnaPopunjenost = popunjenostPoDanima
                .Select(p => new PopunjenostIzvjestajViewModel.PopunjenostDnevna
                {
                    Datum = p.Key,
                    BrojZauzetihMjesta = (int)(p.Value * ukupnoMjesta / 100),
                    BrojSlobodnihMjesta =
                        ukupnoMjesta - (int)(p.Value * ukupnoMjesta / 100),
                    PopunjenostProcenat = p.Value,
                })
                .ToList();

            return new PopunjenostIzvjestajViewModel
            {
                ParkingId = parkingId,
                ParkingNaziv = parkingNaziv,
                PeriodOd = od,
                PeriodDo = doo,
                UkupnoMjesta = ukupnoMjesta,
                UkupnoRezervacija = await _izvjestajRepository.DohvatiBrojRezervacijaZaPeriodAsync(
                    parkingId,
                    od,
                    doo
                ),
                ProsjecnaPopunjenost = prosjecna,
                MaksimalnaPopunjenost = maksimalna,
                MinimalnaPopunjenost = minimalna,
                DnevnaPopunjenost = dnevnaPopunjenost,
            };
        }

        public async Task<KorisniciIzvjestajViewModel> GenerisiKorisniciIzvjestajAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            string parkingNaziv;
            if (parkingId == 0)
            {
                parkingNaziv = "Svi parking prostori";
            }
            else
            {
                var parking = await _parkingRepository.DohvatiPoIdAsync(parkingId);
                if (parking == null)
                    throw new KeyNotFoundException($"Parking sa ID {parkingId} nije pronađen");
                parkingNaziv = parking.Naziv;
            }

            var ukupnoKorisnika = await _izvjestajRepository.DohvatiBrojKorisnikaZaPeriodAsync(parkingId, od, doo);
            var noviKorisnici = await _izvjestajRepository.DohvatiNoveKorisnikeZaPeriodAsync(od, doo);
            var ukupnoRezervacija = await _izvjestajRepository.DohvatiBrojRezervacijaZaPeriodAsync(parkingId, od, doo);

            var aktivniKorisniciPoDanima = await _izvjestajRepository.DohvatiAktivneKorisnikePoDanimaAsync(parkingId, od, doo);
            var noveRegistracijePoDanima = await _izvjestajRepository.DohvatiNoveRegistracijePoDanimaAsync(od, doo);

            var dnevnaStatistika = aktivniKorisniciPoDanima.Select(ak => new KorisniciIzvjestajViewModel.KorisniciDnevna
            {
                Datum = ak.Key,
                BrojAktivnihKorisnika = ak.Value,
                BrojNoveRegistracije = noveRegistracijePoDanima.GetValueOrDefault(ak.Key, 0)
            }).ToList();

            return new KorisniciIzvjestajViewModel
            {
                ParkingId = parkingId,
                ParkingNaziv = parkingNaziv,
                PeriodOd = od,
                PeriodDo = doo,
                UkupnoKorisnika = ukupnoKorisnika,
                NoviKorisnici = noviKorisnici,
                UkupnoRezervacija = ukupnoRezervacija,
                DnevnaStatistika = dnevnaStatistika
            };
        }

        public async Task<PrihodiIzvjestajViewModel> GenerisiPrihodiIzvjestajAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            string parkingNaziv;
            if (parkingId == 0)
            {
                parkingNaziv = "Svi parking prostori";
            }
            else
            {
                var parking = await _parkingRepository.DohvatiPoIdAsync(parkingId);
                if (parking == null)
                    throw new KeyNotFoundException($"Parking sa ID {parkingId} nije pronađen");
                parkingNaziv = parking.Naziv;
            }

            var prihodiPoDanima = await _izvjestajRepository.DohvatiPrihodePoDanimaAsync(
                parkingId,
                od,
                doo
            );
            var ukupnoRezervacija = await _izvjestajRepository.DohvatiBrojRezervacijaZaPeriodAsync(
                parkingId,
                od,
                doo
            );
            var ukupniPrihod = await _izvjestajRepository.DohvatiPrihodZaPeriodAsync(
                parkingId,
                od,
                doo
            );

            var brojDana = (doo - od).Days + 1;
            var prosjecniDnevniPrihod = brojDana > 0 ? ukupniPrihod / brojDana : 0;

            var dnevniPrihodi = new List<PrihodiIzvjestajViewModel.PrihodiDnevna>();

            foreach (var p in prihodiPoDanima)
            {
                var brojRezervacija =
                    await _izvjestajRepository.DohvatiBrojRezervacijaZaPeriodAsync(
                        parkingId,
                        p.Key,
                        p.Key.AddDays(1).AddSeconds(-1)
                    );

                dnevniPrihodi.Add(
                    new PrihodiIzvjestajViewModel.PrihodiDnevna
                    {
                        Datum = p.Key,
                        BrojRezervacija = brojRezervacija,
                        Prihod = p.Value,
                    }
                );
            }

            return new PrihodiIzvjestajViewModel
            {
                ParkingId = parkingId,
                ParkingNaziv = parkingNaziv,
                PeriodOd = od,
                PeriodDo = doo,
                UkupnoRezervacija = ukupnoRezervacija,
                UkupniPrihod = ukupniPrihod,
                ProsjecniDnevniPrihod = prosjecniDnevniPrihod,
                MaksimalniDnevniPrihod = prihodiPoDanima.Any() ? prihodiPoDanima.Values.Max() : 0,
                MinimalniDnevniPrihod = prihodiPoDanima.Any() ? prihodiPoDanima.Values.Min() : 0,
                ProsjecnaCijenaPoRezervaciji =
                    ukupnoRezervacija > 0 ? ukupniPrihod / ukupnoRezervacija : 0,
                DnevniPrihodi = dnevniPrihodi,
            };
        }

        public async Task<bool> ObrisiIzvjestajAsync(int id)
        {
            var izvjestaj = await _izvjestajRepository.DohvatiPoIdAsync(id);
            if (izvjestaj == null)
                return false;

            _izvjestajRepository.Obrisi(izvjestaj);
            await _izvjestajRepository.SacuvajPromjeneAsync();
            return true;
        }

        public async Task<IzvjestajListaViewModel> DohvatiListuIzvjestajaViewModelAsync(
            int? parkingFilter = null,
            TipIzvjestaja? tipFilter = null
        )
        {
            var izvjestaji = await _izvjestajRepository.DohvatiSveAsync();
            var lista = izvjestaji
                .Select(i => new IzvjestajOsnovniViewModel
                {
                    IzvjestajId = i.IzvjestajId,
                    DatumGenerisanja = i.DatumGenerisanja,
                    PeriodOd = i.PeriodOd,
                    PeriodDo = i.PeriodDo,
                    UkupnoRezervacija = i.UkupnoRezervacija,
                    UkupniPrihod = i.UkupniPrihod,
                    ParkingId = i.ParkingId,
                    ParkingNaziv = i.Parking?.Naziv ?? string.Empty,
                    TipIzvjestaja = i.TipIzvjestaja,
                })
                .ToList();

            if (parkingFilter.HasValue)
                lista = lista.Where(i => i.ParkingId == parkingFilter.Value).ToList();

            if (tipFilter.HasValue)
                lista = lista.Where(i => i.TipIzvjestaja == tipFilter.Value).ToList();

            return new IzvjestajListaViewModel
            {
                Izvjestaji = lista,
                UkupnoIzvjestaja = lista.Count,
                ParkingFilter = parkingFilter,
                TipFilter = tipFilter,
                DostupniParkinzi = await _izvjestajRepository.DohvatiSveParkingeZaSelectListAsync(),
            };
        }

        public async Task<IzvjestajDetaljiViewModel?> DohvatiDetaljeIzvjestajaViewModelAsync(int id)
        {
            var izvjestaj = await _izvjestajRepository.DohvatiPoIdAsync(id);
            if (izvjestaj == null)
                return null;

            var dnevneStatistike = new List<DnevnaStatistika>();
            var rezervacijePoDanima = await _izvjestajRepository.DohvatiRezervacijePoDanimaAsync(
                izvjestaj.ParkingId,
                izvjestaj.PeriodOd,
                izvjestaj.PeriodDo
            );
            var prihodiPoDanima = await _izvjestajRepository.DohvatiPrihodePoDanimaAsync(
                izvjestaj.ParkingId,
                izvjestaj.PeriodOd,
                izvjestaj.PeriodDo
            );
            var popunjenostPoDanima = await _izvjestajRepository.DohvatiPopunjenostPoDanimaAsync(
                izvjestaj.ParkingId,
                izvjestaj.PeriodOd,
                izvjestaj.PeriodDo
            );

            foreach (var dan in rezervacijePoDanima)
            {
                dnevneStatistike.Add(
                    new DnevnaStatistika
                    {
                        Datum = dan.Key,
                        BrojRezervacija = dan.Value,
                        Prihod = prihodiPoDanima.GetValueOrDefault(dan.Key),
                        PopunjenostProcenat = popunjenostPoDanima.GetValueOrDefault(dan.Key),
                    }
                );
            }

            var satneStatistike = await _izvjestajRepository.DohvatiRezervacijePoSatimaAsync(
                izvjestaj.ParkingId,
                izvjestaj.PeriodOd,
                izvjestaj.PeriodDo
            );
            var sedmicneStatistike =
                await _izvjestajRepository.DohvatiRezervacijePoDanimaSedmiceAsync(
                    izvjestaj.ParkingId,
                    izvjestaj.PeriodOd,
                    izvjestaj.PeriodDo
                );

            return new IzvjestajDetaljiViewModel
            {
                IzvjestajId = izvjestaj.IzvjestajId,
                DatumGenerisanja = izvjestaj.DatumGenerisanja,
                PeriodOd = izvjestaj.PeriodOd,
                PeriodDo = izvjestaj.PeriodDo,
                UkupnoRezervacija = izvjestaj.UkupnoRezervacija,
                UkupniPrihod = izvjestaj.UkupniPrihod,
                ParkingId = izvjestaj.ParkingId,
                ParkingNaziv = izvjestaj.Parking?.Naziv ?? string.Empty,
                ParkingAdresa = izvjestaj.Parking?.Adresa ?? string.Empty,
                ParkingUkupnoMjesta = izvjestaj.Parking?.UkupnoMjesta ?? 0,
                ParkingCijenaPoSatu = izvjestaj.Parking?.CijenaPoSatu ?? 0,
                TipIzvjestaja = izvjestaj.TipIzvjestaja,
                DnevneStatistike = dnevneStatistike.OrderBy(d => d.Datum).ToList(),
                SatneStatistike = satneStatistike
                    .Select(s => new SatnaStatistika { Sat = s.Key, BrojRezervacija = s.Value })
                    .ToList(),
                SedmicneStatistike = sedmicneStatistike
                    .Select(s => new SedmicnaStatistika { Dan = s.Key, BrojRezervacija = s.Value })
                    .ToList(),
            };
        }

        public async Task<IzvjestajKreirajViewModel> DohvatiViewModelZaKreiranjeAsync()
        {
            return new IzvjestajKreirajViewModel
            {
                DostupniParkinzi = await _izvjestajRepository.DohvatiSveParkingeZaSelectListAsync(),
                PeriodOd = DateTime.Now.AddDays(-30),
                PeriodDo = DateTime.Now,
                TipIzvjestaja = TipIzvjestaja.Prihodi,
                SacuvajUzBazi = true,
                GenerisiPdf = false,
                GenerisiExcel = false,
            };
        }

        public async Task<Dictionary<string, decimal>> DohvatiStatistikuPrihodaZaGodinuAsync(
            int godina
        )
        {
            var rezultat = new Dictionary<string, decimal>();
            var pocetak = new DateTime(godina, 1, 1);
            var kraj = new DateTime(godina, 12, 31, 23, 59, 59);

            for (int mjesec = 1; mjesec <= 12; mjesec++)
            {
                var mjesecPocetak = new DateTime(godina, mjesec, 1);
                var mjesecKraj = mjesecPocetak.AddMonths(1).AddSeconds(-1);
                var prihod = await _izvjestajRepository.DohvatiPrihodZaPeriodAsync(
                    0,
                    mjesecPocetak,
                    mjesecKraj
                );
                var naziv = new DateTime(godina, mjesec, 1).ToString("MMM");
                rezultat[naziv] = prihod;
            }

            return rezultat;
        }

        public async Task<Dictionary<string, double>> DohvatiStatistikuPopunjenostiZaGodinuAsync(
            int godina
        )
        {
            var rezultat = new Dictionary<string, double>();
            var parkingIds = (await _parkingRepository.DohvatiSveAsync())
                .Select(p => p.ParkingId)
                .ToList();

            for (int mjesec = 1; mjesec <= 12; mjesec++)
            {
                var mjesecPocetak = new DateTime(godina, mjesec, 1);
                var mjesecKraj = mjesecPocetak.AddMonths(1).AddSeconds(-1);

                double ukupnaPopunjenost = 0;
                foreach (var parkingId in parkingIds)
                {
                    var popunjenost = await _izvjestajRepository.DohvatiProsjecnuPopunjenostAsync(
                        parkingId,
                        mjesecPocetak,
                        mjesecKraj
                    );
                    ukupnaPopunjenost += popunjenost;
                }

                var prosjek = parkingIds.Any() ? ukupnaPopunjenost / parkingIds.Count : 0;
                var naziv = new DateTime(godina, mjesec, 1).ToString("MMM");
                rezultat[naziv] = prosjek;
            }

            return rezultat;
        }

        public async Task<byte[]> GenerisiExcelIzvjestajAsync(int izvjestajId)
        {
            throw new NotImplementedException("Excel generisanje će biti implementirano naknadno");
        }

        public async Task<byte[]> GenerisiPdfIzvjestajAsync(int izvjestajId)
        {
            throw new NotImplementedException("PDF generisanje će biti implementirano naknadno");
        }
    }
}
