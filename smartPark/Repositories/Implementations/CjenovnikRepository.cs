using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Repositories.Interfaces;

namespace smartPark.Repositories.Implementations
{
    public class CjenovnikRepository : ICjenovnikRepository
    {
        private readonly ApplicationDbContext _kontekstBaza;
        private readonly DbSet<Cjenovnik> _cjenovnik;

        public CjenovnikRepository(ApplicationDbContext kontekst)
        {
            _kontekstBaza = kontekst;
            _cjenovnik = kontekst.Cjenovnici;
        }

        public async Task<IEnumerable<Cjenovnik>> DohvatiSveCjenovnikeAsync()
        {
            return await _cjenovnik
                .Include(c => c.Parking)
                .OrderByDescending(c => c.DatumPocetka)
                .ToListAsync();
        }

        public async Task<Cjenovnik?> DohvatiPoIdCjenovnikAsync(int idCjenovnika)
        {
            return await _cjenovnik
                .Include(c => c.Parking)
                .FirstOrDefaultAsync(c => c.CjenovnikId == idCjenovnika);
        }

        public async Task<IEnumerable<Cjenovnik>> PronadjiCjenovnikAsync(
            Expression<Func<Cjenovnik, bool>> uslov
        )
        {
            return await _cjenovnik.Include(c => c.CjenovnikId).Where(uslov).ToListAsync();
        }

        public async Task DodajCjenovnikAsync(Cjenovnik entitet)
        {
            await _cjenovnik.AddAsync(entitet);
        }

        public void IzmjeniCjenovnik(Cjenovnik entitet)
        {
            _cjenovnik.Update(entitet);
        }

        public void ObrisiCjenovnik(Cjenovnik entitet)
        {
            _cjenovnik.Remove(entitet);
        }

        public async Task<IEnumerable<Cjenovnik>> DohvatiSveAktivneCjenovnikeAsync(
            int? parkingId = null
        )
        {
            var upit = _cjenovnik
                .Include(c => c.Parking)
                .Where(c =>
                    c.Aktivan
                    && c.DatumPocetka <= DateTime.Now
                    && (c.DatumKraja == null || c.DatumKraja >= DateTime.Now)
                );

            if (parkingId.HasValue)
            {
                upit = upit.Where(c => c.ParkingId == parkingId.Value);
            }

            return await upit.OrderBy(c => c.Parking!.Naziv).ToListAsync();
        }

        public async Task<Cjenovnik?> DohvatiAktivniCjenovnikZaParkingAsync(
            int parkingId,
            TipPerioda? period = null
        )
        {
            var upit = _cjenovnik
                .Include(c => c.Parking)
                .Where(c =>
                    c.Aktivan
                    && c.DatumPocetka <= DateTime.Now
                    && (c.DatumKraja == null || c.DatumKraja >= DateTime.Now)
                );

            if (period.HasValue)
            {
                upit.Where(c => c.TipPerioda == period.Value);
            }

            return await upit.FirstOrDefaultAsync();
        }

        public async Task<bool> ImaAktivniCjenovnikAsync(
            int parkingId,
            DateTime datumPocetka,
            DateTime? datumKraja = null
        )
        {
            return await _cjenovnik.AnyAsync(c =>
                c.ParkingId == parkingId
                && c.Aktivan
                && c.DatumPocetka <= (datumKraja ?? DateTime.Now)
                && (c.DatumKraja == null || c.DatumKraja >= DateTime.Now)
            );
        }

        public async Task DeaktivirajSveCjenovnikeZaParkingAsync(int parkingId)
        {
            var aktivniCjenovnici = await _cjenovnik
                .Where(c => c.ParkingId == parkingId && c.Aktivan)
                .ToListAsync();

            foreach (var cjenovnik in aktivniCjenovnici)
            {
                cjenovnik.Aktivan = false;
            }
        }

        public async Task<Cjenovnik?> DohvatiPodrazumjevaniCjenovnikZaParkingAsync(int parkingId)
        {
            return await _cjenovnik
                .Where(c => c.ParkingId == parkingId && c.TipPerioda == TipPerioda.Dan)
                .OrderByDescending(c => c.DatumPocetka)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> PostojiCjenovnikAsync(int idCjenovnika)
        {
            return await _cjenovnik.AnyAsync(c => c.CjenovnikId == idCjenovnika);
        }

        public async Task<int> PrebrojiCjenovnikeAsync(
            Expression<Func<Cjenovnik, bool>>? uslov = null
        )
        {
            if (uslov == null)
            {
                return await _cjenovnik.CountAsync();
            }
            return await _cjenovnik.CountAsync(uslov);
        }

        public async Task SacuvajPromjeneAsync()
        {
            await _kontekstBaza.SaveChangesAsync();
        }
    }
}
