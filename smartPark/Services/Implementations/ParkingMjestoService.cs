using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.ParkingMjesto;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class ParkingMjestoService : IParkingMjestoService
    {
        private readonly IParkingMjestoRepository _parkingMjestoRepository;
        private readonly IParkingRepository _parkingRepository;

        public ParkingMjestoService(
            IParkingMjestoRepository parkingMjestoRepository,
            IParkingRepository parkingRepository
        )
        {
            _parkingMjestoRepository = parkingMjestoRepository;
            _parkingRepository = parkingRepository;
        }

        public async Task<ParkingMjesto?> DohvatiParkingMjestoPoIdAsync(int id)
        {
            return await _parkingMjestoRepository.DohvatiPoIdAsync(id);
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiSvaParkingMjestaAsync()
        {
            return await _parkingMjestoRepository.DohvatiSveAsync();
        }

        public async Task<ParkingMjesto> KreirajParkingMjestoAsync(
            ParkingMjestoKreirajViewModel model
        )
        {
            if (await BrojMjestaVecPostojiUParkinguAsync(model.ParkingId, model.BrojMjesta))
            {
                throw new InvalidOperationException(
                    $"Parking već ima mjesto sa brojem {model.BrojMjesta}"
                );
            }

            var parking = await _parkingRepository.DohvatiPoIdAsync(model.ParkingId);
            if (parking == null)
            {
                throw new KeyNotFoundException($"Parking sa ID {model.ParkingId} nije pronađen");
            }

            var parkingMjesto = new ParkingMjesto
            {
                ParkingId = model.ParkingId,
                BrojMjesta = model.BrojMjesta,
                StatusMjesta = model.StatusMjesta,
            };

            await _parkingMjestoRepository.DodajAsync(parkingMjesto);
            await _parkingMjestoRepository.SacuvajPromjeneAsync();

            return parkingMjesto;
        }

        public async Task<List<ParkingMjesto>> KreirajViseParkingMjestaAsync(
            ParkingMjestoKreirajViewModel model
        )
        {
            var kreirana = new List<ParkingMjesto>();

            for (int i = 1; i <= model.BrojZaKreiranje; i++)
            {
                var postojeciBroj = i;
                if (!await BrojMjestaVecPostojiUParkinguAsync(model.ParkingId, postojeciBroj))
                {
                    var mjesto = new ParkingMjesto
                    {
                        ParkingId = model.ParkingId,
                        BrojMjesta = postojeciBroj,
                        StatusMjesta = StatusMjesta.Slobodno,
                    };
                    await _parkingMjestoRepository.DodajAsync(mjesto);
                    kreirana.Add(mjesto);
                }
            }

            await _parkingMjestoRepository.SacuvajPromjeneAsync();
            return kreirana;
        }

        public async Task<ParkingMjesto> AzurirajParkingMjestoAsync(
            ParkingMjestoUrediViewModel model
        )
        {
            var postojeci = await _parkingMjestoRepository.DohvatiPoIdAsync(model.ParkingMjestoId);
            if (postojeci == null)
            {
                throw new KeyNotFoundException(
                    $"Parking mjesto sa ID {model.ParkingMjestoId} nije pronađeno"
                );
            }

            if (postojeci.BrojMjesta != model.BrojMjesta)
            {
                if (
                    await BrojMjestaVecPostojiUParkinguAsync(
                        postojeci.ParkingId,
                        model.BrojMjesta,
                        model.ParkingMjestoId
                    )
                )
                {
                    throw new InvalidOperationException(
                        $"Parking već ima mjesto sa brojem {model.BrojMjesta}"
                    );
                }
            }

            postojeci.BrojMjesta = model.BrojMjesta;
            postojeci.StatusMjesta = model.StatusMjesta;

            _parkingMjestoRepository.Izmijeni(postojeci);
            await _parkingMjestoRepository.SacuvajPromjeneAsync();

            return postojeci;
        }

        public async Task<bool> ObrisiParkingMjestoAsync(int id)
        {
            var mjesto = await _parkingMjestoRepository.DohvatiPoIdAsync(id);
            if (mjesto == null)
                return false;

            _parkingMjestoRepository.Obrisi(mjesto);
            await _parkingMjestoRepository.SacuvajPromjeneAsync();
            return true;
        }

        public async Task<bool> PromijeniStatusAsync(ParkingMjestoPromjenaStatusaViewModel model)
        {
            return await _parkingMjestoRepository.AzurirajStatusAsync(
                model.ParkingMjestoId,
                model.NoviStatus
            );
        }

        public async Task<bool> OslobodiMjestoAsync(int id)
        {
            return await _parkingMjestoRepository.OslobodiMjestoAsync(id);
        }

        public async Task<bool> ZauzmiMjestoAsync(int id, int rezervacijaId)
        {
            return await _parkingMjestoRepository.DodijeliRezervacijuMjestuAsync(id, rezervacijaId);
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiMjestaPoParkinguAsync(int parkingId)
        {
            return await _parkingMjestoRepository.DohvatiPoParkinguAsync(parkingId);
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiSlobodnaMjestaPoParkinguAsync(
            int parkingId
        )
        {
            return await _parkingMjestoRepository.DohvatiSlobodnaMjestaPoParkinguAsync(parkingId);
        }

        public async Task<ParkingMjestoOsnovniViewModel?> DohvatiPrvoSlobodnoMjestoPoParkinguAsync(
            int parkingId
        )
        {
            var mjesto = await _parkingMjestoRepository.DohvatiPrvoSlobodnoMjestoPoParkinguAsync(
                parkingId
            );
            if (mjesto == null)
                return null;

            return new ParkingMjestoOsnovniViewModel
            {
                ParkingMjestoId = mjesto.ParkingMjestoId,
                ParkingId = mjesto.ParkingId,
                ParkingNaziv = mjesto.Parking?.Naziv ?? string.Empty,
                BrojMjesta = mjesto.BrojMjesta,
                StatusMjesta = mjesto.StatusMjesta,
            };
        }

        public async Task<int> DohvatiBrojSlobodnihMjestaPoParkinguAsync(int parkingId)
        {
            return await _parkingMjestoRepository.DohvatiBrojSlobodnihMjestaPoParkinguAsync(
                parkingId
            );
        }

        public async Task<int> DohvatiBrojZauzetihMjestaPoParkinguAsync(int parkingId)
        {
            return await _parkingMjestoRepository.DohvatiBrojZauzetihMjestaPoParkinguAsync(
                parkingId
            );
        }

        public async Task<Dictionary<StatusMjesta, int>> DohvatiStatistikuPoParkinguAsync(
            int parkingId
        )
        {
            return await _parkingMjestoRepository.DohvatiStatistikuPoParkinguAsync(parkingId);
        }

        public async Task<ParkingMjestoListaViewModel> DohvatiListuParkingMjestaViewModelAsync(
            int? parkingFilter = null,
            string? statusFilter = null
        )
        {
            IEnumerable<ParkingMjesto> mjesta;

            if (parkingFilter.HasValue)
            {
                mjesta = await _parkingMjestoRepository.DohvatiPoParkinguAsync(parkingFilter.Value);
            }
            else
            {
                mjesta = await _parkingMjestoRepository.DohvatiSveAsync();
            }

            var lista = mjesta.ToList();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                var status = Enum.Parse<StatusMjesta>(statusFilter);
                lista = lista.Where(m => m.StatusMjesta == status).ToList();
            }

            var viewModelLista = lista
                .Select(m => new ParkingMjestoOsnovniViewModel
                {
                    ParkingMjestoId = m.ParkingMjestoId,
                    ParkingId = m.ParkingId,
                    ParkingNaziv = m.Parking?.Naziv ?? string.Empty,
                    BrojMjesta = m.BrojMjesta,
                    StatusMjesta = m.StatusMjesta,
                })
                .ToList();

            return new ParkingMjestoListaViewModel
            {
                ParkingMjesta = viewModelLista,
                UkupnoMjesta = viewModelLista.Count,
                SlobodnihMjesta = viewModelLista.Count(m =>
                    m.StatusMjesta == StatusMjesta.Slobodno
                ),
                ZauzetihMjesta = viewModelLista.Count(m => m.StatusMjesta == StatusMjesta.Zauzeto),
                RezervisanihMjesta = viewModelLista.Count(m =>
                    m.StatusMjesta == StatusMjesta.Zauzeto
                ),
                NedostupnihMjesta = viewModelLista.Count(m =>
                    m.StatusMjesta == StatusMjesta.Zauzeto
                ),
                ParkingFilter = parkingFilter,
                StatusFilter = statusFilter,
            };
        }

        public async Task<ParkingMjestoDetaljiViewModel?> DohvatiDetaljeParkingMjestaViewModelAsync(
            int id
        )
        {
            var mjesto = await _parkingMjestoRepository.DohvatiPoIdSaRezervacijomAsync(id);
            if (mjesto == null)
                return null;

            return new ParkingMjestoDetaljiViewModel
            {
                ParkingMjestoId = mjesto.ParkingMjestoId,
                ParkingId = mjesto.ParkingId,
                ParkingNaziv = mjesto.Parking?.Naziv ?? string.Empty,
                ParkingAdresa = mjesto.Parking?.Adresa ?? string.Empty,
                ParkingCijenaPoSatu = mjesto.Parking?.CijenaPoSatu ?? 0,
                ParkingTip = mjesto.Parking?.TipParkinga.ToString() ?? string.Empty,
                BrojMjesta = mjesto.BrojMjesta,
                StatusMjesta = mjesto.StatusMjesta,
                TrenutnaRezervacijaId = mjesto.TrenutnaRezervacija?.RezervacijaId.ToString(),
                TrenutniKorisnikIme = mjesto.TrenutnaRezervacija?.Korisnik?.Ime,
                TrenutniKorisnikPrezime = mjesto.TrenutnaRezervacija?.Korisnik?.Prezime,
                RezervacijaPocetak = mjesto.TrenutnaRezervacija?.PocetakRezervacije,
                RezervacijaKraj = mjesto.TrenutnaRezervacija?.KrajRezervacije,
            };
        }

        public async Task<ParkingMjestoKreirajViewModel> DohvatiViewModelZaKreiranjeAsync()
        {
            return new ParkingMjestoKreirajViewModel
            {
                DostupniParkinzi =
                    await _parkingMjestoRepository.DohvatiSveParkingeZaSelectListAsync(),
                StatusMjesta = StatusMjesta.Slobodno,
                KreirajViseMjesta = false,
                BrojZaKreiranje = 1,
            };
        }

        public async Task<ParkingMjestoUrediViewModel?> DohvatiViewModelZaUredjivanjeAsync(int id)
        {
            var mjesto = await _parkingMjestoRepository.DohvatiPoIdAsync(id);
            if (mjesto == null)
                return null;

            return new ParkingMjestoUrediViewModel
            {
                ParkingMjestoId = mjesto.ParkingMjestoId,
                ParkingId = mjesto.ParkingId,
                ParkingNaziv = mjesto.Parking?.Naziv ?? string.Empty,
                BrojMjesta = mjesto.BrojMjesta,
                StatusMjesta = mjesto.StatusMjesta,
            };
        }

        public async Task<bool> BrojMjestaVecPostojiUParkinguAsync(
            int parkingId,
            int brojMjesta,
            int? izuzmiId = null
        )
        {
            return await _parkingMjestoRepository.PostojiLiBrojMjestaUParkinguAsync(
                parkingId,
                brojMjesta,
                izuzmiId
            );
        }

        public async Task<bool> ParkingMjestoPostojiAsync(int id)
        {
            return await _parkingMjestoRepository.PostojiLiAsync(id);
        }

        // Prosirenje kapaciteta — dodaje nova mjesta do novog ukupnog broja
        public async Task<(bool Uspjeh, string Poruka)> ProsiriKapacitetAsync(int parkingId, int brNovihMjesta)
        {
            if (brNovihMjesta <= 0)
                return (false, "Broj novih mjesta mora biti veći od 0.");

            var parking = await _parkingRepository.DohvatiPoIdAsync(parkingId);
            if (parking == null)
                return (false, "Parking nije pronađen.");

            var postojecaMjesta = (await _parkingMjestoRepository.DohvatiPoParkinguAsync(parkingId)).ToList();
            var maxBroj = postojecaMjesta.Any() ? postojecaMjesta.Max(m => m.BrojMjesta) : 0;

            int kreirana = 0;
            for (int i = 1; i <= brNovihMjesta; i++)
            {
                var noviBroj = maxBroj + i;
                var novo = new ParkingMjesto
                {
                    ParkingId = parkingId,
                    BrojMjesta = noviBroj,
                    StatusMjesta = StatusMjesta.Slobodno,
                };
                await _parkingMjestoRepository.DodajAsync(novo);
                kreirana++;
            }

            // Azuriraj UkupnoMjesta na parkingu
            parking.UkupnoMjesta += kreirana;
            _parkingRepository.Izmijeni(parking);

            await _parkingMjestoRepository.SacuvajPromjeneAsync();
            return (true, $"Uspješno dodano {kreirana} novih parking mjesta. Novi kapacitet: {parking.UkupnoMjesta}.");
        }

        // Smanjenje kapaciteta — brise slobodna mjesta od najveceg broja ka manjem
        public async Task<(bool Uspjeh, string Poruka)> SmanjiKapacitetAsync(int parkingId, int brMjestaZaUkloniti)
        {
            if (brMjestaZaUkloniti <= 0)
                return (false, "Broj mjesta za uklanjanje mora biti veći od 0.");

            var parking = await _parkingRepository.DohvatiPoIdAsync(parkingId);
            if (parking == null)
                return (false, "Parking nije pronađen.");

            var slobodnaMjesta = (await _parkingMjestoRepository.DohvatiPoParkinguAsync(parkingId))
                .Where(m => m.StatusMjesta == StatusMjesta.Slobodno)
                .OrderByDescending(m => m.BrojMjesta)
                .ToList();

            if (slobodnaMjesta.Count == 0)
                return (false, "Nema slobodnih mjesta za uklanjanje. Sva mjesta su trenutno zauzeta.");

            if (brMjestaZaUkloniti > slobodnaMjesta.Count)
                return (false, $"Možete ukloniti najviše {slobodnaMjesta.Count} slobodnih mjesta (zauzeta mjesta se ne mogu ukloniti).");

            int uklonjeno = 0;
            foreach (var mjesto in slobodnaMjesta.Take(brMjestaZaUkloniti))
            {
                _parkingMjestoRepository.Obrisi(mjesto);
                uklonjeno++;
            }

            // Azuriraj UkupnoMjesta na parkingu
            parking.UkupnoMjesta -= uklonjeno;
            _parkingRepository.Izmijeni(parking);

            await _parkingMjestoRepository.SacuvajPromjeneAsync();
            return (true, $"Uspješno uklonjeno {uklonjeno} parking mjesta. Novi kapacitet: {parking.UkupnoMjesta}.");
        }
    }
}
