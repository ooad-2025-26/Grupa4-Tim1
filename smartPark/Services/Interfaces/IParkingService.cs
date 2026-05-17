using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Parking.Admin;
using smartPark.Models.ViewModels.Parking.Menadzer;

namespace smartPark.Services.Interfaces
{
    public interface IParkingService
    {
        // ========== OSNOVNE RADNJE ==========
        Task<Parking?> DohvatiParkingPoIdAsync(int id);
        Task<IEnumerable<Parking>> DohvatiSveParkingeAsync();
        Task<IEnumerable<Parking>> DohvatiAktivneParkingeAsync();

        // ========== ADMIN RADNJE ==========
        Task<AdminParkingListaViewModel> DohvatiAdminListuParkingaAsync(
            string? filterStatus = null,
            string? filterTip = null
        );
        Task<AdminParkingDetaljiViewModel?> DohvatiAdminDetaljeParkingaAsync(int id);
        Task<AdminParkingKreirajViewModel> DohvatiAdminViewModelZaKreiranjeAsync();
        Task<AdminParkingUrediViewModel?> DohvatiAdminViewModelZaUredjivanjeAsync(int id);
        Task<AdminParkingStatistikaViewModel> DohvatiAdminStatistikuParkingaAsync();

        Task<Parking> AdminKreirajParkingAsync(AdminParkingKreirajViewModel model);
        Task<Parking?> AdminAzurirajParkingAsync(AdminParkingUrediViewModel model);
        Task<bool> AdminObrisiParkingAsync(int id);

        // ========== MENADŽER RADNJE ==========
        Task<MenadzerParkingDetaljiViewModel?> DohvatiMenadzerParkingDetaljiAsync(
            string menadzerId
        );
        Task<MenadzerParkingStatistikaViewModel?> DohvatiMenadzerStatistikuParkingaAsync(
            string menadzerId
        );
        Task<MenadzerParkingUrediViewModel?> DohvatiMenadzerViewModelZaUredjivanjeAsync(
            string menadzerId
        );
        Task<Parking?> MenadzerAzurirajParkingAsync(MenadzerParkingUrediViewModel model);

        // ========== ZAJEDNIČKE RADNJE ==========
        Task<bool> ParkingPostojiAsync(int id);
        Task<bool> NazivParkingaPostojiAsync(string naziv, int? izuzmiId = null);
        Task<int> DohvatiBrojSlobodnihMjestaAsync(int parkingId);
        Task<decimal> IzracunajCijenuAsync(int parkingId, DateTime pocetak, DateTime kraj);

        // ========== ZA DROPDOWN LISTE ==========
        Task<
            IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        > DohvatiSveMenadzereZaSelectListAsync();
    }
}
