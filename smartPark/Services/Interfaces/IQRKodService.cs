using smartPark.Models.ViewModels.Rezervacija;

namespace smartPark.Services.Interfaces
{
    public interface IQRKodService
    {
        Task<QRKodViewModel> GenerisiQRKodZaRezervacijuAsync(int rezervacijaId);
        Task<QRKodViewModel?> DohvatiQRKodPoRezervacijiAsync(int rezervacijaId);
        Task<bool> ValidirajQRKodAsync(string kod);
        Task<smartPark.Models.Entities.QRKod?> DohvatiQRKodPoKoduAsync(string kod);
        Task<bool> IskoristiQRKodAsync(int rezervacijaId);
        Task<bool> PonistiIskoristenjeAsync(int rezervacijaId);
    }
}
