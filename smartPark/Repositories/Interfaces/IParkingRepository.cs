using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Parking.Admin;
using smartPark.Models.ViewModels.Parking.Menadzer;

namespace smartPark.Repositories.Interfaces
{
    public interface IParkingRepository
    {
        // ========== OSNOVNE RADNJE ==========
        Task<Parking?> DohvatiPoIdAsync(int id);
        Task<Parking?> DohvatiPoIdSaRezervacijamaAsync(int id);
        Task<Parking?> DohvatiPoIdSaParkingMjestimaAsync(int id);
        Task<IEnumerable<Parking>> DohvatiSveAsync();
        Task<IEnumerable<Parking>> DohvatiSveSaMenadzerimaAsync();
        Task<IEnumerable<Parking>> DohvatiAktivneAsync();
        Task<IEnumerable<Parking>> PronadjiAsync(Expression<Func<Parking, bool>> uslov);

        // ========== RADNJE ZA DODAVANJE, IZMJENU I BRISANJE ==========
        Task DodajAsync(Parking parking);
        void Izmijeni(Parking parking);
        void Obrisi(Parking parking);
        Task SacuvajPromjeneAsync();

        // ========== RADNJE ZA MENADŽERA ==========
        Task<Parking?> DohvatiParkingPoMenadzeruAsync(string menadzerId);
        Task<bool> DaLiMenadzerUpravljaParkingomAsync(string menadzerId, int parkingId);

        // ========== RADNJE ZA STATISTIKU ==========
        Task<int> DohvatiBrojRezervacijaZaParkingAsync(
            int parkingId,
            DateTime? od = null,
            DateTime? doo = null
        );
        Task<decimal> DohvatiPrihodZaParkingAsync(
            int parkingId,
            DateTime? od = null,
            DateTime? doo = null
        );
        Task<double> DohvatiProsjecnuZauzetostAsync(
            int parkingId,
            DateTime? od = null,
            DateTime? doo = null
        );
        Task<Dictionary<int, int>> DohvatiRezervacijePoSatimaAsync(
            int parkingId,
            DateTime? datum = null
        );
        Task<Dictionary<DayOfWeek, int>> DohvatiRezervacijePoDanimaSedmiceAsync(int parkingId);

        // ========== DODATNE METODE ZA STATISTIKU ==========
        Task<int> PrebrojRezervacijeAsync();
        Task<decimal> DohvatiUkupniPrihodAsync();
        Task<List<Rezervacija>> DohvatiPosljednjeRezervacijeKorisnikaAsync(
            string korisnikId,
            int broj
        );

        // ========== ZA ADMIN DASHBOARD ==========
        Task<int> DohvatiUkupnoParkingaAsync();
        Task<int> DohvatiBrojAktivnihParkingaAsync();
        Task<int> DohvatiUkupnoMjestaAsync();
        Task<int> DohvatiUkupnoSlobodnihMjestaAsync();
        Task<decimal> DohvatiUkupniPrihodZaPeriodAsync(DateTime od, DateTime doo);
        Task<
            List<AdminParkingStatistikaViewModel.NajpopularnijiParking>
        > DohvatiNajpopularnijeParkingeAsync(int broj = 5);
        Task<Dictionary<DateTime, int>> DohvatiRezervacijePoDanimaAsync(
            DateTime? od = null,
            DateTime? doo = null
        );
        Task<Dictionary<DateTime, decimal>> DohvatiPrihodePoDanimaAsync(
            DateTime? od = null,
            DateTime? doo = null
        );

        // ========== SPECIFIČNE RADNJE ZA MENADŽERA ==========
        Task<
            List<MenadzerParkingDetaljiViewModel.AktivnaRezervacija>
        > DohvatiAktivneRezervacijeZaParkingAsync(int parkingId);
        Task<int> DohvatiBrojAktivnihRezervacijaTrenutnoAsync(int parkingId);

        // ========== POMOĆNE RADNJE ==========
        Task<bool> PostojiLiAsync(int id);
        Task<bool> PostojiLiNazivAsync(string naziv, int? izuzmiId = null);
        Task<int> PrebrojAsync();
        Task<Dictionary<TipParkinga, int>> DohvatiBrojParkingaPoTipuAsync();
        Task<Dictionary<TipParkinga, decimal>> DohvatiProsjecnuCijenuPoTipuAsync();

        // ========== ZA DROPDOWN LISTE ==========
        Task<IEnumerable<SelectListItem>> DohvatiSveMenadzereZaSelectListAsync();
    }
}
