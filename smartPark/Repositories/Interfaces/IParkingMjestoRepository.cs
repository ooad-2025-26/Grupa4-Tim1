using System.Linq.Expressions;
using smartPark.Models.Entities;
using smartPark.Models.Enums;

namespace smartPark.Repositories.Interfaces
{
    public interface IParkingMjestoRepository
    {
        // ========== OSNOVNE RADNJE ==========
        Task<ParkingMjesto?> DohvatiPoIdAsync(int id);
        Task<ParkingMjesto?> DohvatiPoIdSaRezervacijomAsync(int id);
        Task<IEnumerable<ParkingMjesto>> DohvatiSveAsync();
        Task<IEnumerable<ParkingMjesto>> DohvatiSveSaParkingomAsync();
        Task<IEnumerable<ParkingMjesto>> PronadjiAsync(Expression<Func<ParkingMjesto, bool>> uslov);

        // ========== RADNJE ZA DODAVANJE, IZMJENU I BRISANJE ==========
        Task DodajAsync(ParkingMjesto parkingMjesto);
        void Izmijeni(ParkingMjesto parkingMjesto);
        void Obrisi(ParkingMjesto parkingMjesto);
        Task SacuvajPromjeneAsync();

        // ========== SPECIFIČNE RADNJE ZA PARKING MJESTA ==========
        Task<IEnumerable<ParkingMjesto>> DohvatiPoParkinguAsync(int parkingId);
        Task<IEnumerable<ParkingMjesto>> DohvatiSlobodnaMjestaPoParkinguAsync(int parkingId);
        Task<IEnumerable<ParkingMjesto>> DohvatiZauzetaMjestaPoParkinguAsync(int parkingId);
        Task<IEnumerable<ParkingMjesto>> DohvatiRezervisanaMjestaPoParkinguAsync(int parkingId);
        Task<ParkingMjesto?> DohvatiPrvoSlobodnoMjestoPoParkinguAsync(int parkingId);
        Task<int> DohvatiBrojSlobodnihMjestaPoParkinguAsync(int parkingId);
        Task<int> DohvatiBrojZauzetihMjestaPoParkinguAsync(int parkingId);
        Task<int> DohvatiBrojRezervisanihMjestaPoParkinguAsync(int parkingId);

        // ========== RADNJE ZA STATUS ==========
        Task<bool> AzurirajStatusAsync(int id, StatusMjesta noviStatus);
        Task<bool> AzurirajStatusPoParkinguAsync(int parkingId, StatusMjesta noviStatus);

        // ========== RADNJE ZA REZERVACIJE ==========
        Task<bool> DodijeliRezervacijuMjestuAsync(int parkingMjestoId, int rezervacijaId);
        Task<bool> OslobodiMjestoAsync(int parkingMjestoId);

        // ========== POMOĆNE RADNJE ==========
        Task<bool> PostojiLiAsync(int id);
        Task<bool> PostojiLiBrojMjestaUParkinguAsync(
            int parkingId,
            int brojMjesta,
            int? izuzmiId = null
        );
        Task<int> PrebrojPoParkinguAsync(int parkingId);
        Task<Dictionary<StatusMjesta, int>> DohvatiStatistikuPoParkinguAsync(int parkingId);

        // ========== ZA DROPDOWN LISTE ==========
        Task<
            IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        > DohvatiSveParkingeZaSelectListAsync();
    }
}
