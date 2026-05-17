using System.Linq.Expressions;
using smartPark.Models.Entities;
using smartPark.Models.Enums;

namespace smartPark.Repositories.Interfaces
{
    public interface ICjenovnikRepository
    {
        Task<IEnumerable<Cjenovnik>> DohvatiSveCjenovnikeAsync();

        Task<Cjenovnik?> DohvatiPoIdCjenovnikAsync(int idCjenovnika);

        Task<IEnumerable<Cjenovnik>> PronadjiCjenovnikAsync(
            Expression<Func<Cjenovnik, bool>> uslov
        );

        Task DodajCjenovnikAsync(Cjenovnik entitet);

        void IzmjeniCjenovnik(Cjenovnik entitet);

        void ObrisiCjenovnik(Cjenovnik entitet);

        Task<IEnumerable<Cjenovnik>> DohvatiSveAktivneCjenovnikeAsync(int? parkingId = null);

        Task<Cjenovnik?> DohvatiAktivniCjenovnikZaParkingAsync(
            int parkingId,
            TipPerioda? period = null
        );

        Task<bool> ImaAktivniCjenovnikAsync(
            int parkingId,
            DateTime datumPocetka,
            DateTime? datumKraja = null
        );

        Task DeaktivirajSveCjenovnikeZaParkingAsync(int parkingId);

        Task<Cjenovnik?> DohvatiPodrazumjevaniCjenovnikZaParkingAsync(int parkingId);

        Task<bool> PostojiCjenovnikAsync(int idCjenovnika);

        Task<int> PrebrojiCjenovnikeAsync(Expression<Func<Cjenovnik, bool>>? uslov = null);

        Task SacuvajPromjeneAsync();
    }
}
