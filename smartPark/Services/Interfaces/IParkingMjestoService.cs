using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.ParkingMjesto;

namespace smartPark.Services.Interfaces
{
    public interface IParkingMjestoService
    {
        Task<ParkingMjesto?> DohvatiParkingMjestoPoIdAsync(int id);
        Task<IEnumerable<ParkingMjesto>> DohvatiSvaParkingMjestaAsync();

        Task<ParkingMjesto> KreirajParkingMjestoAsync(ParkingMjestoKreirajViewModel model);
        Task<List<ParkingMjesto>> KreirajViseParkingMjestaAsync(
            ParkingMjestoKreirajViewModel model
        );
        Task<ParkingMjesto> AzurirajParkingMjestoAsync(ParkingMjestoUrediViewModel model);
        Task<bool> ObrisiParkingMjestoAsync(int id);

        Task<bool> PromijeniStatusAsync(ParkingMjestoPromjenaStatusaViewModel model);
        Task<bool> OslobodiMjestoAsync(int id);
        Task<bool> ZauzmiMjestoAsync(int id, int rezervacijaId);

        Task<IEnumerable<ParkingMjesto>> DohvatiMjestaPoParkinguAsync(int parkingId);
        Task<IEnumerable<ParkingMjesto>> DohvatiSlobodnaMjestaPoParkinguAsync(int parkingId);
        Task<ParkingMjestoOsnovniViewModel?> DohvatiPrvoSlobodnoMjestoPoParkinguAsync(
            int parkingId
        );

        Task<int> DohvatiBrojSlobodnihMjestaPoParkinguAsync(int parkingId);
        Task<int> DohvatiBrojZauzetihMjestaPoParkinguAsync(int parkingId);
        Task<Dictionary<StatusMjesta, int>> DohvatiStatistikuPoParkinguAsync(int parkingId);

        Task<ParkingMjestoListaViewModel> DohvatiListuParkingMjestaViewModelAsync(
            int? parkingFilter = null,
            string? statusFilter = null
        );
        Task<ParkingMjestoDetaljiViewModel?> DohvatiDetaljeParkingMjestaViewModelAsync(int id);
        Task<ParkingMjestoKreirajViewModel> DohvatiViewModelZaKreiranjeAsync();
        Task<ParkingMjestoUrediViewModel?> DohvatiViewModelZaUredjivanjeAsync(int id);

        Task<bool> BrojMjestaVecPostojiUParkinguAsync(
            int parkingId,
            int brojMjesta,
            int? izuzmiId = null
        );
        Task<bool> ParkingMjestoPostojiAsync(int id);
    }
}
