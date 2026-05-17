using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Cjenovnik;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class CjenovnikService : ICjenovnikService
    {
        private readonly ICjenovnikRepository _cjenovikRepozitorij;

        public CjenovnikService(ICjenovnikRepository cjenovnikRepozitorij)
        {
            _cjenovikRepozitorij = cjenovnikRepozitorij;
        }

        public async Task<IEnumerable<Cjenovnik>> DohvatiSveCjenovnikeAsync()
        {
            return await _cjenovikRepozitorij.DohvatiSveCjenovnikeAsync();
        }

        public async Task<Cjenovnik?> DohvatiCjenovnikPoIdAsync(int cjenovnikId)
        {
            return await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(cjenovnikId);
        }

        public async Task<Cjenovnik> KreirajCjenovnikAsync(CjenovnikKreirajViewModel model)
        {
            if (!await MozeLiSeKreiratiCjenovnikAsync(model))
            {
                throw new InvalidOperationException(
                    "Cjenovnik se ne može kreirati. Provjerite datume i parking."
                );
            }

            var cjenovnik = new Cjenovnik
            {
                ParkingId = model.ParkingId,
                CijenaPoSatu = model.CijenaPoSatu,
                Zona = model.Zona,
                TipPerioda = model.TipPerioda,
                DatumPocetka = model.DatumPocetka,
                DatumKraja = model.DatumKraja,
                Aktivan = true,
            };

            await _cjenovikRepozitorij.DodajCjenovnikAsync(cjenovnik);
            await _cjenovikRepozitorij.SacuvajPromjeneAsync();

            return cjenovnik;
        }

        public async Task<Cjenovnik> AzurirajCjenovnikAsync(CjenovnikUrediViewModel model)
        {
            var postojeci = await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(model.CjenovnikId);

            if (postojeci == null)
            {
                throw new KeyNotFoundException(
                    $"Cjenovnik sa ID: {model.CjenovnikId} nije pronađen."
                );
            }

            postojeci.CijenaPoSatu = model.CijenaPoSatu;
            postojeci.Zona = model.Zona;
            postojeci.TipPerioda = model.TipPerioda;
            postojeci.DatumPocetka = model.DatumPocetka;
            postojeci.DatumKraja = model.DatumKraja;

            _cjenovikRepozitorij.IzmjeniCjenovnik(postojeci);
            await _cjenovikRepozitorij.SacuvajPromjeneAsync();

            return postojeci;
        }

        public async Task<bool> ObrisiCjenovnikAsync(int cjenovnikId)
        {
            var cjenovnikZaObrisati = await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(
                cjenovnikId
            );

            if (cjenovnikZaObrisati == null)
            {
                return false;
            }

            _cjenovikRepozitorij.ObrisiCjenovnik(cjenovnikZaObrisati);
            await _cjenovikRepozitorij.SacuvajPromjeneAsync();

            return true;
        }

        public async Task<bool> DeaktivirajCjenovnikAsync(int cjenovnikId)
        {
            var postojeci = await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(cjenovnikId);

            if (postojeci == null)
            {
                throw new KeyNotFoundException($"Cjenovnik sa ID: {cjenovnikId} nije pronađen.");
            }

            if (!await MozeLiSeDeaktiviratiCjenovnikAsync(cjenovnikId))
            {
                throw new InvalidOperationException(
                    "Ne možete deaktivirati cjenovnik koji je jedini aktivni za ovaj parking."
                );
            }

            postojeci.Aktivan = false;

            _cjenovikRepozitorij.IzmjeniCjenovnik(postojeci);
            await _cjenovikRepozitorij.SacuvajPromjeneAsync();

            return true;
        }

        public async Task<Cjenovnik> AzurirajCijenuCjenovnikaAsync(
            int cjenovnikId,
            decimal novaCijena
        )
        {
            if (novaCijena < 0)
            {
                throw new InvalidOperationException("Cijena mora biti veća od 0.");
            }

            var cjenovnik = await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(cjenovnikId);

            if (cjenovnik == null)
            {
                throw new KeyNotFoundException($"Cjenovnik sa ID: {cjenovnikId} nije pronađen.");
            }

            cjenovnik.CijenaPoSatu = novaCijena;
            _cjenovikRepozitorij.IzmjeniCjenovnik(cjenovnik);
            await _cjenovikRepozitorij.SacuvajPromjeneAsync();

            return cjenovnik;
        }

        public async Task<decimal> PrimjeniCijenuCjenovnikaAsync(
            int parkingId,
            int sati,
            TipPerioda period
        )
        {
            var cjenovnik = await _cjenovikRepozitorij.DohvatiAktivniCjenovnikZaParkingAsync(
                parkingId,
                period
            );
            if (cjenovnik == null)
            {
                cjenovnik = await _cjenovikRepozitorij.DohvatiAktivniCjenovnikZaParkingAsync(
                    parkingId,
                    TipPerioda.Dan
                );
            }

            if (cjenovnik == null)
            {
                throw new InvalidOperationException(
                    $"Nema aktivnog cjenovnika za parking ID: {parkingId}"
                );
            }

            return cjenovnik.CijenaPoSatu * sati;
        }

        public async Task<bool> MozeLiSeKreiratiCjenovnikAsync(CjenovnikKreirajViewModel model)
        {
            var parking = await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(model.ParkingId);

            if (parking == null)
            {
                return false;
            }

            if (
                await DaLiSeCjenovnikPreklapaAsync(
                    model.ParkingId,
                    model.DatumPocetka,
                    model.DatumKraja
                )
            )
            {
                return false;
            }

            if (model.DatumKraja.HasValue && model.DatumKraja <= model.DatumPocetka)
            {
                return false;
            }

            if (model.CijenaPoSatu <= 0)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> MozeLiSeDeaktiviratiCjenovnikAsync(int cjenovnikId)
        {
            var cjenovnik = await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(cjenovnikId);
            if (cjenovnik == null)
            {
                return false;
            }

            var aktivnihBroj = await _cjenovikRepozitorij.PrebrojiCjenovnikeAsync(c =>
                c.ParkingId == cjenovnik.ParkingId && c.Aktivan
            );

            return aktivnihBroj > 1;
        }

        public async Task<decimal> DohvatiTrenutnuCijenuZaParkingAsync(
            int parkingId,
            TipPerioda period = TipPerioda.Dan
        )
        {
            var cjenovnik = await _cjenovikRepozitorij.DohvatiAktivniCjenovnikZaParkingAsync(
                parkingId,
                period
            );
            if (cjenovnik == null)
            {
                return 0;
            }

            return cjenovnik.CijenaPoSatu;
        }

        public async Task<CjenovnikListaViewModel> DohvatiListuCjenovnikaViewModelAsync(
            int? parkingId = null
        )
        {
            IEnumerable<Cjenovnik> cjenovnici;

            if (parkingId.HasValue)
            {
                cjenovnici = await _cjenovikRepozitorij.PronadjiCjenovnikAsync(c =>
                    c.ParkingId == parkingId.Value
                );
            }
            else
            {
                cjenovnici = await _cjenovikRepozitorij.DohvatiSveCjenovnikeAsync();
            }

            var lista = cjenovnici.ToList();

            return new CjenovnikListaViewModel
            {
                Cjenovnici = lista,
                UkupnoCjenovnika = lista.Count,
                AktivnihCjenovnika = lista.Count(c => c.Aktivan),
                ParkingFilter = parkingId,
            };
        }

        public async Task<CjenovnikDetaljiViewModel> DohvatiDetaljeCjenovnikaViewModelAsync(
            int cjenovnikId
        )
        {
            var cjenovnik = await _cjenovikRepozitorij.DohvatiPoIdCjenovnikAsync(cjenovnikId);

            if (cjenovnik == null)
            {
                return null!;
            }
            return new CjenovnikDetaljiViewModel
            {
                CjenovnikId = cjenovnik.CjenovnikId,
                ParkingNaziv = cjenovnik.Parking?.Naziv ?? "Nepoznat",
                CijenaPoSatu = cjenovnik.CijenaPoSatu,
                Zona = cjenovnik.Zona,
                TipPerioda = cjenovnik.TipPerioda,
                DatumPocetka = cjenovnik.DatumPocetka,
                DatumKraja = cjenovnik.DatumKraja,
                Aktivan = cjenovnik.Aktivan,
                JeVazeci =
                    cjenovnik.Aktivan
                    && cjenovnik.DatumPocetka <= DateTime.Now
                    && (!cjenovnik.DatumKraja.HasValue || cjenovnik.DatumKraja >= DateTime.Now),
            };
        }

        public async Task<bool> DaLiSeCjenovnikPreklapaAsync(
            int parkingId,
            DateTime pocetak,
            DateTime? kraj,
            int? izuzmiId = null
        )
        {
            var postojeci = await _cjenovikRepozitorij.PronadjiCjenovnikAsync(c =>
                c.ParkingId == parkingId
                && (!izuzmiId.HasValue || c.CjenovnikId != izuzmiId.Value)
                && c.DatumPocetka <= (kraj ?? DateTime.MaxValue)
                && (c.DatumKraja == null || c.DatumKraja >= pocetak)
            );

            return postojeci.Any();
        }
    }
}
