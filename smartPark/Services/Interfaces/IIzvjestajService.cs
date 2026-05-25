using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Izvjestaj;

namespace smartPark.Services.Interfaces
{
    public interface IIzvjestajService
    {
        Task<Izvjestaj?> DohvatiIzvjestajPoIdAsync(int id);
        Task<IEnumerable<Izvjestaj>> DohvatiSveIzvjestajeAsync();

        Task<Izvjestaj> GenerisiIzvjestajAsync(IzvjestajKreirajViewModel model);
        Task<PopunjenostIzvjestajViewModel> GenerisiPopunjenostIzvjestajAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );
        Task<PrihodiIzvjestajViewModel> GenerisiPrihodiIzvjestajAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );
        Task<KorisniciIzvjestajViewModel> GenerisiKorisniciIzvjestajAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );

        Task<bool> ObrisiIzvjestajAsync(int id);

        Task<IzvjestajListaViewModel> DohvatiListuIzvjestajaViewModelAsync(
            int? parkingFilter = null,
            TipIzvjestaja? tipFilter = null
        );
        Task<IzvjestajDetaljiViewModel?> DohvatiDetaljeIzvjestajaViewModelAsync(int id);
        Task<IzvjestajKreirajViewModel> DohvatiViewModelZaKreiranjeAsync();

        Task<Dictionary<string, decimal>> DohvatiStatistikuPrihodaZaGodinuAsync(int godina);
        Task<Dictionary<string, double>> DohvatiStatistikuPopunjenostiZaGodinuAsync(int godina);

        Task<byte[]> GenerisiExcelIzvjestajAsync(int izvjestajId);
        Task<byte[]> GenerisiPdfIzvjestajAsync(int izvjestajId);
    }
}
