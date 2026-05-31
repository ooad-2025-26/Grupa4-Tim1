using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Rezervacija;

namespace smartPark.Services.Interfaces
{
    public interface IRezervacijaService
    {
        Task<Rezervacija?> DohvatiRezervacijuPoIdAsync(int id);
        Task<IEnumerable<Rezervacija>> DohvatiSveRezervacijeAsync();

        Task<Rezervacija> KreirajRezervacijuAsync(
            RezervacijaKreirajViewModel model,
            string korisnikId
        );
        Task<Rezervacija> AzurirajRezervacijuAsync(RezervacijaUrediViewModel model);
        Task<bool> OtkaziRezervacijuAsync(RezervacijaOtkaziViewModel model);
        Task<bool> ProduziRezervacijuAsync(int rezervacijaId, int dodatnoMinuta);
        Task<bool> ObrisiRezervacijuAsync(int id);

        Task<IEnumerable<Rezervacija>> DohvatiRezervacijeKorisnikaAsync(string korisnikId);
        Task<RezervacijaListaViewModel> DohvatiMojeRezervacijeViewModelAsync(string korisnikId);
        Task<bool> KorisnikImaAktivnuRezervacijuUPerioduAsync(string korisnikId, DateTime pocetak, DateTime kraj);

        Task<bool> ProvjeriDostupnostParkingaAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        );
        Task<bool> ProvjeriDostupnostMjestaAsync(
            int parkingMjestoId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        );
        Task<ParkingMjesto?> DohvatiPrvoSlobodnoMjestoAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj
        );
        Task<IEnumerable<ParkingMjesto>> DohvatiSlobodnaMjestaZaPeriodAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj
        );

        Task<RezervacijaListaViewModel> DohvatiListuRezervacijaViewModelAsync(
            int? parkingFilter = null,
            string? statusFilter = null,
            DateTime? datumOd = null,
            DateTime? datumDo = null
        );
        Task<RezervacijaDetaljiViewModel?> DohvatiDetaljeRezervacijeViewModelAsync(int id);
        Task<QRKodViewModel?> DohvatiQRKodZaRezervacijuAsync(int id);
        Task<RezervacijaKreirajViewModel> DohvatiViewModelZaKreiranjeAsync();
        Task<RezervacijaUrediViewModel?> DohvatiViewModelZaUredjivanjeAsync(int id);
        Task<RezervacijaOtkaziViewModel?> DohvatiViewModelZaOtkazivanjeAsync(int id);

        Task<decimal> DohvatiUkupniPrihodAsync();
        Task<Dictionary<StatusRezervacije, int>> DohvatiStatistikuPoStatusuAsync();
    }
}
