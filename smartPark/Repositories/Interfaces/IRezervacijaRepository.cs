using System.Linq.Expressions;
using smartPark.Models.Entities;
using smartPark.Models.Enums;

namespace smartPark.Repositories.Interfaces
{
    public interface IRezervacijaRepository
    {
        // ========== OSNOVNE RADNJE ==========
        Task<Rezervacija?> DohvatiPoIdAsync(int id);
        Task<Rezervacija?> DohvatiPoIdSaSvimeAsync(int id);
        Task<IEnumerable<Rezervacija>> DohvatiSveAsync();
        Task<IEnumerable<Rezervacija>> DohvatiSveSaSvimeAsync();
        Task<IEnumerable<Rezervacija>> PronadjiAsync(Expression<Func<Rezervacija, bool>> uslov);

        // ========== RADNJE ZA DODAVANJE, IZMJENU I BRISANJE ==========
        Task DodajAsync(Rezervacija rezervacija);
        void Izmijeni(Rezervacija rezervacija);
        void Obrisi(Rezervacija rezervacija);
        Task SacuvajPromjeneAsync();

        // ========== SPECIFIČNE RADNJE ==========
        Task<IEnumerable<Rezervacija>> DohvatiPoKorisnikuAsync(string korisnikId);
        Task<IEnumerable<Rezervacija>> DohvatiPoParkinguAsync(int parkingId);
        Task<IEnumerable<Rezervacija>> DohvatiAktivneRezervacijeAsync();
        Task<IEnumerable<Rezervacija>> DohvatiAktivneRezervacijeZaParkingAsync(int parkingId);
        Task<IEnumerable<Rezervacija>> DohvatiRezervacijeZaPeriodAsync(
            DateTime pocetak,
            DateTime kraj
        );
        Task<bool> PostojiLiPreklapanjeAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        );
        Task<bool> PostojiLiPreklapanjeZaMjestoAsync(
            int parkingMjestoId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        );

        // ========== STATISTIKA ==========
        Task<int> DohvatiBrojRezervacijaPoStatusuAsync(StatusRezervacije status);
        Task<Dictionary<DateTime, int>> DohvatiRezervacijePoDanimaAsync(int brojDana = 30);
        Task<decimal> DohvatiUkupniPrihodZaPeriodAsync(DateTime od, DateTime doo);

        // ========== ZA DROPDOWN LISTE ==========
        Task<
            IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        > DohvatiSlobodnaParkingMjestaZaSelectListAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj
        );
    }
}
