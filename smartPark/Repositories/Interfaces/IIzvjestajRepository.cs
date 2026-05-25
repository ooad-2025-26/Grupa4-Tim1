using System.Linq.Expressions;
using smartPark.Models.Entities;
using smartPark.Models.Enums;

namespace smartPark.Repositories.Interfaces
{
    public interface IIzvjestajRepository
    {
        // ========== OSNOVNE RADNJE ==========
        Task<Izvjestaj?> DohvatiPoIdAsync(int id);
        Task<IEnumerable<Izvjestaj>> DohvatiSveAsync();
        Task<IEnumerable<Izvjestaj>> DohvatiPoParkinguAsync(int parkingId);
        Task<IEnumerable<Izvjestaj>> DohvatiPoTipuAsync(TipIzvjestaja tip);
        Task<IEnumerable<Izvjestaj>> PronadjiAsync(Expression<Func<Izvjestaj, bool>> uslov);

        // ========== RADNJE ZA DODAVANJE, IZMJENU I BRISANJE ==========
        Task DodajAsync(Izvjestaj izvjestaj);
        void Izmijeni(Izvjestaj izvjestaj);
        void Obrisi(Izvjestaj izvjestaj);
        Task SacuvajPromjeneAsync();

        // ========== RADNJE ZA GENERISANJE IZVJEŠTAJA ==========
        Task<int> DohvatiBrojRezervacijaZaPeriodAsync(int parkingId, DateTime od, DateTime doo);
        Task<decimal> DohvatiPrihodZaPeriodAsync(int parkingId, DateTime od, DateTime doo);
        Task<Dictionary<DateTime, int>> DohvatiRezervacijePoDanimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );
        Task<Dictionary<DateTime, decimal>> DohvatiPrihodePoDanimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );
        Task<Dictionary<int, int>> DohvatiRezervacijePoSatimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );
        Task<Dictionary<DayOfWeek, int>> DohvatiRezervacijePoDanimaSedmiceAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );

        // ========== RADNJE ZA POPUNJENOST ==========
        Task<Dictionary<DateTime, double>> DohvatiPopunjenostPoDanimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        );
        Task<double> DohvatiProsjecnuPopunjenostAsync(int parkingId, DateTime od, DateTime doo);
        Task<double> DohvatiMaksimalnuPopunjenostAsync(int parkingId, DateTime od, DateTime doo);
        Task<double> DohvatiMinimalnuPopunjenostAsync(int parkingId, DateTime od, DateTime doo);

        // ========== POMOĆNE RADNJE ==========
        Task<bool> PostojiLiAsync(int id);
        Task<int> PrebrojAsync();
        Task<int> PrebrojPoParkinguAsync(int parkingId);

        // ========== RADNJE ZA KORISNIKE ==========
        Task<int> DohvatiBrojKorisnikaZaPeriodAsync(int parkingId, DateTime od, DateTime doo);
        Task<int> DohvatiNoveKorisnikeZaPeriodAsync(DateTime od, DateTime doo);
        Task<Dictionary<DateTime, int>> DohvatiAktivneKorisnikePoDanimaAsync(int parkingId, DateTime od, DateTime doo);
        Task<Dictionary<DateTime, int>> DohvatiNoveRegistracijePoDanimaAsync(DateTime od, DateTime doo);

        // ========== ZA DROPDOWN LISTE ==========
        Task<
            IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        > DohvatiSveParkingeZaSelectListAsync();
    }
}
