using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using smartPark.Helpers;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Rezervacija;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class RezervacijaService : IRezervacijaService
    {
        private readonly IRezervacijaRepository _rezervacijaRepository;
        private readonly IParkingRepository _parkingRepository;
        private readonly IParkingMjestoRepository _parkingMjestoRepository;
        private readonly IQRKodService _qrKodService;
        private readonly ICjenovnikRepository _cjenovnikRepository;
        private readonly IEmailService _emailService;
        private readonly UserManager<Korisnik> _userManager;

        public RezervacijaService(
            IRezervacijaRepository rezervacijaRepository,
            IParkingRepository parkingRepository,
            IParkingMjestoRepository parkingMjestoRepository,
            IQRKodService qrKodService,
            ICjenovnikRepository cjenovnikRepository,
            IEmailService emailService,
            UserManager<Korisnik> userManager
        )
        {
            _rezervacijaRepository = rezervacijaRepository;
            _parkingRepository = parkingRepository;
            _parkingMjestoRepository = parkingMjestoRepository;
            _qrKodService = qrKodService;
            _cjenovnikRepository = cjenovnikRepository;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<Rezervacija?> DohvatiRezervacijuPoIdAsync(int id)
        {
            return await _rezervacijaRepository.DohvatiPoIdAsync(id);
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiSveRezervacijeAsync()
        {
            return await _rezervacijaRepository.DohvatiSveAsync();
        }

        public async Task<Rezervacija> KreirajRezervacijuAsync(
            RezervacijaKreirajViewModel model,
            string korisnikId
        )
        {
            var parking = await _parkingRepository.DohvatiPoIdAsync(model.ParkingId);
            if (parking == null)
                throw new KeyNotFoundException($"Parking sa ID {model.ParkingId} nije pronađen");

            // Provjera radnog vremena — samo sat pocetka rezervacije mora biti unutar radnog vremena
            // Visednevne rezervacije su dozvoljene
            if (!string.IsNullOrEmpty(parking.RadnoVrijeme))
            {
                var match = System.Text.RegularExpressions.Regex.Match(parking.RadnoVrijeme, @"(\d{1,2}):(\d{2})\s*-\s*(\d{1,2}):(\d{2})");
                if (match.Success)
                {
                    var radnoOd = new TimeSpan(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), 0);

                    int doSati = int.Parse(match.Groups[3].Value);
                    int doMinuta = int.Parse(match.Groups[4].Value);
                    var radnoDo = doSati == 24 ? new TimeSpan(23, 59, 59) : new TimeSpan(doSati, doMinuta, 0);

                    var pocetakTime = model.PocetakRezervacije.TimeOfDay;

                    // Provjeri samo sat pocetka rezervacije
                    if (pocetakTime < radnoOd || pocetakTime > radnoDo)
                    {
                        throw new InvalidOperationException($"Pocetak rezervacije je izvan radnog vremena parking prostora ({parking.RadnoVrijeme})");
                    }

                    // Ako je jednodnevna rezervacija, provjeri i sat kraja
                    if (model.PocetakRezervacije.Date == model.KrajRezervacije.Date)
                    {
                        var krajTime = model.KrajRezervacije.TimeOfDay;
                        if (krajTime < radnoOd || krajTime > radnoDo)
                        {
                            throw new InvalidOperationException($"Kraj rezervacije je izvan radnog vremena parking prostora ({parking.RadnoVrijeme})");
                        }
                    }
                }
            }

            // Napomena: Provjera dostupnosti se vrsi ispod kroz DohvatiPrvoSlobodnoMjestoAsync
            // koji korektno trazi slobodno mjesto, a ne samo provjerava da li postoji ijedna rezervacija

            // Odabir parking mjesta
            int? parkingMjestoId = model.ParkingMjestoId;

            if (!parkingMjestoId.HasValue)
            {
                var slobodnoMjesto = await DohvatiPrvoSlobodnoMjestoAsync(
                    model.ParkingId,
                    model.PocetakRezervacije,
                    model.KrajRezervacije
                );
                if (slobodnoMjesto != null)
                {
                    parkingMjestoId = slobodnoMjesto.ParkingMjestoId;
                }
                else
                {
                    throw new InvalidOperationException("Nema slobodnih parking mjesta na ovom parkingu u odabranom terminu");
                }
            }
            else
            {
                // Provjera dostupnosti odabranog mjesta
                if (
                    !await ProvjeriDostupnostMjestaAsync(
                        parkingMjestoId.Value,
                        model.PocetakRezervacije,
                        model.KrajRezervacije
                    )
                )
                {
                    throw new InvalidOperationException(
                        "Odabrano parking mjesto nije dostupno u odabranom terminu"
                    );
                }
            }



            var bazaCijena = await IzracunajCijenuRezervacijeAsync(model.ParkingId, model.PocetakRezervacije, model.KrajRezervacije);
            var ukupnaCijena = bazaCijena * (1 - model.Popust / 100m);

            var rezervacija = new Rezervacija
            {
                KorisnikId = korisnikId,
                ParkingId = model.ParkingId,
                ParkingMjestoId = parkingMjestoId,
                PocetakRezervacije = model.PocetakRezervacije,
                KrajRezervacije = model.KrajRezervacije,
                UkupnaCijena = ukupnaCijena,
                StatusRezervacije = StatusRezervacije.Aktivna,
                DatumKreiranjaRezervacije = DateTime.UtcNow,
            };

            await _rezervacijaRepository.DodajAsync(rezervacija);
            await _rezervacijaRepository.SacuvajPromjeneAsync();

            // Generiši QR kod
            await _qrKodService.GenerisiQRKodZaRezervacijuAsync(rezervacija.RezervacijaId);

            // Ažuriraj status parking mjesta ako je dodijeljeno i ako rezervacija počinje ODMAH
            if (parkingMjestoId.HasValue)
            {
                var sada = DateTime.Now;
                if (model.PocetakRezervacije <= sada && model.KrajRezervacije >= sada)
                {
                    await _parkingMjestoRepository.AzurirajStatusAsync(
                        parkingMjestoId.Value,
                        StatusMjesta.Zauzeto
                    );
                }
                else
                {
                    await _parkingMjestoRepository.AzurirajStatusAsync(
                        parkingMjestoId.Value,
                        StatusMjesta.Slobodno
                    );
                }

                // Ponovo izračunaj i snimi slobodna mjesta
                var zauzetaSada = await _rezervacijaRepository.PronadjiAsync(r =>
                    r.ParkingId == parking.ParkingId &&
                    r.StatusRezervacije == StatusRezervacije.Aktivna &&
                    r.PocetakRezervacije <= sada &&
                    r.KrajRezervacije >= sada
                );
                parking.SlobodnaMjesta = Math.Max(0, parking.UkupnoMjesta - zauzetaSada.Count());
                _parkingRepository.Izmijeni(parking);
                await _parkingRepository.SacuvajPromjeneAsync();
            }

            // Pošalji email potvrde korisniku
            try
            {
                var korisnik = await _userManager.FindByIdAsync(korisnikId);
                if (korisnik?.Email != null)
                {
                    await _emailService.PosaljiPotvrduRezervacijeAsync(
                        korisnik.Email,
                        $"{korisnik.Ime} {korisnik.Prezime}",
                        rezervacija.RezervacijaId,
                        parking.Naziv,
                        rezervacija.PocetakRezervacije,
                        rezervacija.KrajRezervacije,
                        rezervacija.UkupnaCijena
                    );
                }
            }
            catch { /* email greska ne bi trebala blokirati rezervaciju, samo dalje */ }

            return rezervacija;
        }

        public async Task<Rezervacija> AzurirajRezervacijuAsync(RezervacijaUrediViewModel model)
        {
            var postojeci = await _rezervacijaRepository.DohvatiPoIdAsync(model.RezervacijaId);
            if (postojeci == null)
                throw new KeyNotFoundException(
                    $"Rezervacija sa ID {model.RezervacijaId} nije pronađena"
                );

            // Provjera preklapanja (izuzimajući trenutnu rezervaciju)
            var postojiPreklapanje = await _rezervacijaRepository.PostojiLiPreklapanjeAsync(
                postojeci.ParkingId,
                model.PocetakRezervacije,
                model.KrajRezervacije,
                model.RezervacijaId
            );

            if (postojiPreklapanje)
                throw new InvalidOperationException(
                    "Termin se preklapa sa postojećom rezervacijom"
                );

            var bazaCijena = await IzracunajCijenuRezervacijeAsync(postojeci.ParkingId, model.PocetakRezervacije, model.KrajRezervacije);
            var ukupnaCijena = bazaCijena * (1 - model.Popust / 100m);

            postojeci.PocetakRezervacije = model.PocetakRezervacije;
            postojeci.KrajRezervacije = model.KrajRezervacije;
            postojeci.UkupnaCijena = ukupnaCijena;
            postojeci.StatusRezervacije = model.StatusRezervacije;
            postojeci.ParkingMjestoId = model.ParkingMjestoId;

            _rezervacijaRepository.Izmijeni(postojeci);
            await _rezervacijaRepository.SacuvajPromjeneAsync();

            return postojeci;
        }

        public async Task<bool> OtkaziRezervacijuAsync(RezervacijaOtkaziViewModel model)
        {
            var rezervacija = await _rezervacijaRepository.DohvatiPoIdAsync(model.RezervacijaId);
            if (rezervacija == null)
                return false;

            if (rezervacija.StatusRezervacije != StatusRezervacije.Aktivna)
                return false;

            rezervacija.StatusRezervacije = StatusRezervacije.Otkazana;
            _rezervacijaRepository.Izmijeni(rezervacija);
            await _rezervacijaRepository.SacuvajPromjeneAsync();

            // Oslobodi parking mjesto ako je bilo dodijeljeno
            if (rezervacija.ParkingMjestoId.HasValue)
            {
                await _parkingMjestoRepository.AzurirajStatusAsync(
                    rezervacija.ParkingMjestoId.Value,
                    StatusMjesta.Slobodno
                );

                // Vrati slobodno mjesto na parkingu
                var parking = await _parkingRepository.DohvatiPoIdAsync(rezervacija.ParkingId);
                if (parking != null)
                {
                    parking.SlobodnaMjesta = parking.SlobodnaMjesta + 1;
                    _parkingRepository.Izmijeni(parking);
                    await _parkingRepository.SacuvajPromjeneAsync();
                }
            }

            if (rezervacija.Korisnik?.Email != null)
            {
                try
                {
                    await _emailService.PosaljiObavijestPrekidaRezervacijeAsync(
                        rezervacija.Korisnik.Email,
                        $"{rezervacija.Korisnik.Ime} {rezervacija.Korisnik.Prezime}",
                        rezervacija.RezervacijaId,
                        rezervacija.Parking?.Naziv ?? "Parking",
                        "otkazana"
                    );
                }
                catch (Exception)
                {
                    // Ignorisemo email greske jer ne bi trebali da blokiramo aplikaciju zbog emaila
                }
            }

            return true;
        }

        public async Task<bool> ProduziRezervacijuAsync(int rezervacijaId, int dodatnoMinuta)
        {
            var rezervacija = await _rezervacijaRepository.DohvatiPoIdAsync(rezervacijaId);
            if (rezervacija == null)
                return false;

            var noviKraj = rezervacija.KrajRezervacije.AddMinutes(dodatnoMinuta);

            // Provjera preklapanja (izuzimajući trenutnu rezervaciju)
            if (rezervacija.ParkingMjestoId.HasValue)
            {
                var postojiPreklapanje = await _rezervacijaRepository.PostojiLiPreklapanjeAsync(
                    rezervacija.ParkingId,
                    rezervacija.PocetakRezervacije,
                    noviKraj,
                    rezervacija.RezervacijaId
                );
                if (postojiPreklapanje)
                    throw new InvalidOperationException("Produženi termin se preklapa sa postojećom rezervacijom!");
            }

            // Provjera radnog vremena parkinga za novi krajnji termin
            var parking = await _parkingRepository.DohvatiPoIdAsync(rezervacija.ParkingId);
            if (parking != null && !string.IsNullOrEmpty(parking.RadnoVrijeme))
            {
                var match = System.Text.RegularExpressions.Regex.Match(parking.RadnoVrijeme, @"(\d{1,2}):(\d{2})\s*-\s*(\d{1,2}):(\d{2})");
                if (match.Success)
                {
                    var radnoOd = new TimeSpan(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), 0);
                    int doSati = int.Parse(match.Groups[3].Value);
                    int doMinuta = int.Parse(match.Groups[4].Value);
                    var radnoDo = doSati == 24 ? new TimeSpan(23, 59, 59) : new TimeSpan(doSati, doMinuta, 0);

                    var krajTime = noviKraj.TimeOfDay;
                    if (krajTime < radnoOd || krajTime > radnoDo)
                    {
                        throw new InvalidOperationException($"Produženi termin je izvan radnog vremena parking prostora ({parking.RadnoVrijeme})");
                    }
                }
            }

            var dodatnaCijena = await IzracunajCijenuRezervacijeAsync(rezervacija.ParkingId, rezervacija.KrajRezervacije, noviKraj);

            rezervacija.KrajRezervacije = noviKraj;
            rezervacija.UkupnaCijena += dodatnaCijena;

            _rezervacijaRepository.Izmijeni(rezervacija);
            await _rezervacijaRepository.SacuvajPromjeneAsync();

            return true;
        }

        public async Task<bool> ObrisiRezervacijuAsync(int id)
        {
            var rezervacija = await _rezervacijaRepository.DohvatiPoIdAsync(id);
            if (rezervacija == null)
                return false;

            if (rezervacija.StatusRezervacije == StatusRezervacije.Aktivna && rezervacija.ParkingMjestoId.HasValue)
            {
                await _parkingMjestoRepository.AzurirajStatusAsync(
                    rezervacija.ParkingMjestoId.Value,
                    StatusMjesta.Slobodno
                );

                var parking = await _parkingRepository.DohvatiPoIdAsync(rezervacija.ParkingId);
                if (parking != null)
                {
                    parking.SlobodnaMjesta = parking.SlobodnaMjesta + 1;
                    _parkingRepository.Izmijeni(parking);
                    await _parkingRepository.SacuvajPromjeneAsync();
                }
            }

            _rezervacijaRepository.Obrisi(rezervacija);
            await _rezervacijaRepository.SacuvajPromjeneAsync();
            return true;
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiRezervacijeKorisnikaAsync(
            string korisnikId
        )
        {
            return await _rezervacijaRepository.DohvatiPoKorisnikuAsync(korisnikId);
        }

        public async Task<RezervacijaListaViewModel> DohvatiMojeRezervacijeViewModelAsync(
            string korisnikId
        )
        {
            var rezervacije = await _rezervacijaRepository.DohvatiPoKorisnikuAsync(korisnikId);
            var lista = rezervacije
                .Select(r => new RezervacijaOsnovniViewModel
                {
                    RezervacijaId = r.RezervacijaId,
                    KorisnikId = r.KorisnikId,
                    KorisnikIme = r.Korisnik?.Ime ?? string.Empty,
                    KorisnikPrezime = r.Korisnik?.Prezime ?? string.Empty,
                    ParkingId = r.ParkingId,
                    ParkingNaziv = r.Parking?.Naziv ?? string.Empty,
                    ParkingMjestoId = r.ParkingMjestoId,
                    ParkingMjestoBroj = r.ParkingMjesto?.BrojMjesta,
                    PocetakRezervacije = r.PocetakRezervacije,
                    KrajRezervacije = r.KrajRezervacije,
                    UkupnaCijena = r.UkupnaCijena,
                    StatusRezervacije = r.StatusRezervacije,
                    DatumKreiranjaRezervacije = r.DatumKreiranjaRezervacije,
                })
                .ToList();

            return new RezervacijaListaViewModel
            {
                Rezervacije = lista,
                UkupnoRezervacija = lista.Count,
                AktivnihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Aktivna
                ),
                OtkazanihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Otkazana
                ),
                ZavrsenihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Zavrsena
                ),
                IsteklihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Istekla
                ),
            };
        }

        public async Task<bool> ProvjeriDostupnostParkingaAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        )
        {
            return !await _rezervacijaRepository.PostojiLiPreklapanjeAsync(
                parkingId,
                pocetak,
                kraj,
                izuzmiId
            );
        }

        public async Task<bool> ProvjeriDostupnostMjestaAsync(
            int parkingMjestoId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        )
        {
            return !await _rezervacijaRepository.PostojiLiPreklapanjeZaMjestoAsync(
                parkingMjestoId,
                pocetak,
                kraj,
                izuzmiId
            );
        }

        public async Task<ParkingMjesto?> DohvatiPrvoSlobodnoMjestoAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj
        )
        {
            // Provjeri koje rezervacije se preklapaju s traženim periodom
            var zauzetaMjesta = await _rezervacijaRepository.PronadjiAsync(r =>
                r.ParkingId == parkingId
                && r.StatusRezervacije == StatusRezervacije.Aktivna
                && r.PocetakRezervacije < kraj
                && r.KrajRezervacije > pocetak
            );

            var zauzetiIdjevi = zauzetaMjesta
                .Where(r => r.ParkingMjestoId.HasValue)
                .Select(r => r.ParkingMjestoId!.Value)
                .ToList();

            // Uzmi prvo slobodno mjesto koje nije rezervisano u traženom periodu
            // (ne filtriramo po StatusMjesta jer bi to sprijecilo ponovne rezervacije)
            var sva = await _parkingMjestoRepository.PronadjiAsync(pm =>
                pm.ParkingId == parkingId
                && !zauzetiIdjevi.Contains(pm.ParkingMjestoId)
            );

            return sva.OrderBy(pm => pm.BrojMjesta).FirstOrDefault();
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiSlobodnaMjestaZaPeriodAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj
        )
        {
            var zauzetaMjesta = await _rezervacijaRepository.PronadjiAsync(r =>
                r.ParkingId == parkingId
                && r.StatusRezervacije == StatusRezervacije.Aktivna
                && r.PocetakRezervacije < kraj
                && r.KrajRezervacije > pocetak
            );

            var zauzetiIdjevi = zauzetaMjesta.Select(r => r.ParkingMjestoId).ToList();

            return await _parkingMjestoRepository
                .PronadjiAsync(pm =>
                    pm.ParkingId == parkingId
                    && pm.StatusMjesta == StatusMjesta.Slobodno
                    && !zauzetiIdjevi.Contains(pm.ParkingMjestoId)
                );
        }

        public async Task<RezervacijaListaViewModel> DohvatiListuRezervacijaViewModelAsync(
            int? parkingFilter = null,
            string? statusFilter = null,
            DateTime? datumOd = null,
            DateTime? datumDo = null
        )
        {
            var rezervacije = await _rezervacijaRepository.DohvatiSveSaSvimeAsync();
            var lista = rezervacije
                .Select(r => new RezervacijaOsnovniViewModel
                {
                    RezervacijaId = r.RezervacijaId,
                    KorisnikId = r.KorisnikId,
                    KorisnikIme = r.Korisnik?.Ime ?? string.Empty,
                    KorisnikPrezime = r.Korisnik?.Prezime ?? string.Empty,
                    ParkingId = r.ParkingId,
                    ParkingNaziv = r.Parking?.Naziv ?? string.Empty,
                    ParkingMjestoId = r.ParkingMjestoId,
                    ParkingMjestoBroj = r.ParkingMjesto?.BrojMjesta,
                    PocetakRezervacije = r.PocetakRezervacije,
                    KrajRezervacije = r.KrajRezervacije,
                    UkupnaCijena = r.UkupnaCijena,
                    StatusRezervacije = r.StatusRezervacije,
                    DatumKreiranjaRezervacije = r.DatumKreiranjaRezervacije,
                })
                .ToList();

            // Filteri
            if (parkingFilter.HasValue)
                lista = lista.Where(r => r.ParkingId == parkingFilter.Value).ToList();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                var status = Enum.Parse<StatusRezervacije>(statusFilter);
                lista = lista.Where(r => r.StatusRezervacije == status).ToList();
            }

            if (datumOd.HasValue)
                lista = lista.Where(r => r.PocetakRezervacije.Date >= datumOd.Value.Date).ToList();

            if (datumDo.HasValue)
                lista = lista.Where(r => r.KrajRezervacije.Date <= datumDo.Value.Date).ToList();

            var dostupniStatusi = Enum.GetValues<StatusRezervacije>()
                .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
                .Select(x => x.Value);

            return new RezervacijaListaViewModel
            {
                Rezervacije = lista,
                UkupnoRezervacija = lista.Count,
                AktivnihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Aktivna
                ),
                OtkazanihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Otkazana
                ),
                ZavrsenihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Zavrsena
                ),
                IsteklihRezervacija = lista.Count(r =>
                    r.StatusRezervacije == StatusRezervacije.Istekla
                ),
                UkupniPrihod = lista.Sum(r => r.UkupnaCijena),
                ParkingFilter = parkingFilter,
                StatusFilter = statusFilter,
                DatumOd = datumOd,
                DatumDo = datumDo,
                DostupniStatusi = dostupniStatusi,
            };
        }

        public async Task<RezervacijaDetaljiViewModel?> DohvatiDetaljeRezervacijeViewModelAsync(
            int id
        )
        {
            var rezervacija = await _rezervacijaRepository.DohvatiPoIdSaSvimeAsync(id);
            if (rezervacija == null)
                return null;

            var qrKod = await _qrKodService.DohvatiQRKodPoRezervacijiAsync(id);

            return new RezervacijaDetaljiViewModel
            {
                RezervacijaId = rezervacija.RezervacijaId,
                KorisnikId = rezervacija.KorisnikId,
                KorisnikIme = rezervacija.Korisnik?.Ime ?? string.Empty,
                KorisnikPrezime = rezervacija.Korisnik?.Prezime ?? string.Empty,
                ParkingId = rezervacija.ParkingId,
                ParkingNaziv = rezervacija.Parking?.Naziv ?? string.Empty,
                ParkingAdresa = rezervacija.Parking?.Adresa ?? string.Empty,
                ParkingCijenaPoSatu = rezervacija.Parking?.CijenaPoSatu ?? 0,
                ParkingMjestoId = rezervacija.ParkingMjestoId,
                ParkingMjestoBroj = rezervacija.ParkingMjesto?.BrojMjesta,
                PocetakRezervacije = rezervacija.PocetakRezervacije,
                KrajRezervacije = rezervacija.KrajRezervacije,
                UkupnaCijena = rezervacija.UkupnaCijena,
                StatusRezervacije = rezervacija.StatusRezervacije,
                DatumKreiranjaRezervacije = rezervacija.DatumKreiranjaRezervacije,
                QRKodBase64 = qrKod?.Base64Slika,
                QRKodDatumIsteka = qrKod?.DatumIsteka,
                QRKodIskoristen = qrKod?.Iskoristen ?? false,
            };
        }

        public async Task<QRKodViewModel?> DohvatiQRKodZaRezervacijuAsync(int id)
        {
            return await _qrKodService.DohvatiQRKodPoRezervacijiAsync(id);
        }

        public async Task<RezervacijaKreirajViewModel> DohvatiViewModelZaKreiranjeAsync()
        {
            var parkinzi = await _parkingRepository.DohvatiSveAsync();

            return new RezervacijaKreirajViewModel
            {
                DostupniParkinzi = parkinzi.Select(p => new SelectListItem
                {
                    Value = p.ParkingId.ToString(),
                    Text = $"{p.Naziv} - {p.Adresa} ({p.CijenaPoSatu} KM/h)",
                }),
                PocetakRezervacije = DateTime.Now.AddHours(1),
                KrajRezervacije = DateTime.Now.AddHours(2),
                DostupnaParkingMjesta = new List<SelectListItem>(),
            };
        }

        public async Task<RezervacijaUrediViewModel?> DohvatiViewModelZaUredjivanjeAsync(int id)
        {
            var rezervacija = await _rezervacijaRepository.DohvatiPoIdAsync(id);
            if (rezervacija == null)
                return null;

            return new RezervacijaUrediViewModel
            {
                RezervacijaId = rezervacija.RezervacijaId,
                PocetakRezervacije = rezervacija.PocetakRezervacije,
                KrajRezervacije = rezervacija.KrajRezervacije,
                Popust = (int)(
                    (
                        rezervacija.Parking?.CijenaPoSatu
                            * (int)
                                Math.Ceiling(
                                    (
                                        rezervacija.KrajRezervacije - rezervacija.PocetakRezervacije
                                    ).TotalHours
                                )
                        - rezervacija.UkupnaCijena
                    )
                        / (
                            rezervacija.Parking?.CijenaPoSatu
                            * (int)
                                Math.Ceiling(
                                    (
                                        rezervacija.KrajRezervacije - rezervacija.PocetakRezervacije
                                    ).TotalHours
                                )
                            * 100m
                        )
                    ?? 0
                ),
                StatusRezervacije = rezervacija.StatusRezervacije,
                ParkingMjestoId = rezervacija.ParkingMjestoId,
                ParkingId = rezervacija.ParkingId,
                ParkingNaziv = rezervacija.Parking?.Naziv ?? string.Empty,
                CijenaPoSatu = rezervacija.Parking?.CijenaPoSatu ?? 0,
            };
        }

        public async Task<RezervacijaOtkaziViewModel?> DohvatiViewModelZaOtkazivanjeAsync(int id)
        {
            var rezervacija = await _rezervacijaRepository.DohvatiPoIdAsync(id);
            if (rezervacija == null)
                return null;

            return new RezervacijaOtkaziViewModel
            {
                RezervacijaId = rezervacija.RezervacijaId,
                KorisnikIme = rezervacija.Korisnik?.Ime ?? string.Empty,
                ParkingNaziv = rezervacija.Parking?.Naziv ?? string.Empty,
                PocetakRezervacije = rezervacija.PocetakRezervacije,
                KrajRezervacije = rezervacija.KrajRezervacije,
                UkupnaCijena = rezervacija.UkupnaCijena,
            };
        }

        public async Task<decimal> DohvatiUkupniPrihodAsync()
        {
            var rezervacije = await _rezervacijaRepository.DohvatiSveAsync();
            return rezervacije.Sum(r => r.UkupnaCijena);
        }

        public async Task<Dictionary<StatusRezervacije, int>> DohvatiStatistikuPoStatusuAsync()
        {
            var statistika = new Dictionary<StatusRezervacije, int>();

            foreach (StatusRezervacije status in Enum.GetValues(typeof(StatusRezervacije)))
            {
                var broj = await _rezervacijaRepository.DohvatiBrojRezervacijaPoStatusuAsync(
                    status
                );
                statistika[status] = broj;
            }

            return statistika;
        }

        private async Task<decimal> IzracunajCijenuRezervacijeAsync(int parkingId, DateTime pocetak, DateTime kraj)
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
    }
}
