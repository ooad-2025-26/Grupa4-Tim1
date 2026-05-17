using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Cjenovnik;

namespace smartPark.Services.Interfaces
{
    public interface ICjenovnikService
    {
        Task<IEnumerable<Cjenovnik>> DohvatiSveCjenovnikeAsync();

        Task<Cjenovnik?> DohvatiCjenovnikPoIdAsync(int cjenovnikId);

        Task<Cjenovnik> KreirajCjenovnikAsync(CjenovnikKreirajViewModel model);

        Task<Cjenovnik> AzurirajCjenovnikAsync(CjenovnikUrediViewModel model);

        Task<bool> ObrisiCjenovnikAsync(int cjenovnikId);

        Task<bool> DeaktivirajCjenovnikAsync(int cjenovnikId);

        Task<Cjenovnik> AzurirajCijenuCjenovnikaAsync(int cjenovnikId, decimal novaCijena);

        Task<decimal> PrimjeniCijenuCjenovnikaAsync(int parkingId, int sati, TipPerioda period);

        // ViewModeli

        Task<CjenovnikListaViewModel> DohvatiListuCjenovnikaViewModelAsync(int? parkingId = null);

        Task<CjenovnikDetaljiViewModel> DohvatiDetaljeCjenovnikaViewModelAsync(int cjenovnikId);

        Task<bool> DaLiSeCjenovnikPreklapaAsync(
            int parkingId,
            DateTime pocetak,
            DateTime? kraj,
            int? izuzmiId = null
        );
    }
}
