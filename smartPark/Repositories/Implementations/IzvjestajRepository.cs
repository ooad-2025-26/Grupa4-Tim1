using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Repositories.Interfaces;

namespace smartPark.Repositories.Implementations
{
    public class IzvjestajRepository : IIzvjestajRepository
    {
        private readonly ApplicationDbContext _kontekst;
        private readonly DbSet<Izvjestaj> _skup;

        public IzvjestajRepository(ApplicationDbContext kontekst)
        {
            _kontekst = kontekst;
            _skup = kontekst.Izvjestaji;
        }

        public async Task<Izvjestaj?> DohvatiPoIdAsync(int id)
        {
            return await _skup
                .Include(i => i.Parking)
                .FirstOrDefaultAsync(i => i.IzvjestajId == id);
        }

        public async Task<IEnumerable<Izvjestaj>> DohvatiSveAsync()
        {
            return await _skup
                .Include(i => i.Parking)
                .OrderByDescending(i => i.DatumGenerisanja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Izvjestaj>> DohvatiPoParkinguAsync(int parkingId)
        {
            return await _skup
                .Include(i => i.Parking)
                .Where(i => i.ParkingId == parkingId)
                .OrderByDescending(i => i.DatumGenerisanja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Izvjestaj>> DohvatiPoTipuAsync(TipIzvjestaja tip)
        {
            return await _skup
                .Include(i => i.Parking)
                .Where(i => i.TipIzvjestaja == tip)
                .OrderByDescending(i => i.DatumGenerisanja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Izvjestaj>> PronadjiAsync(
            Expression<Func<Izvjestaj, bool>> uslov
        )
        {
            return await _skup.Include(i => i.Parking).Where(uslov).ToListAsync();
        }

        public async Task DodajAsync(Izvjestaj izvjestaj)
        {
            await _skup.AddAsync(izvjestaj);
        }

        public void Izmijeni(Izvjestaj izvjestaj)
        {
            _skup.Update(izvjestaj);
        }

        public void Obrisi(Izvjestaj izvjestaj)
        {
            _skup.Remove(izvjestaj);
        }

        public async Task SacuvajPromjeneAsync()
        {
            await _kontekst.SaveChangesAsync();
        }

        public async Task<int> DohvatiBrojRezervacijaZaPeriodAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            return await _kontekst
                .Rezervacije.Where(r =>
                    (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo
                    && r.StatusRezervacije != StatusRezervacije.Otkazana
                )
                .CountAsync();
        }

        public async Task<decimal> DohvatiPrihodZaPeriodAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            return await _kontekst
                .Rezervacije.Where(r =>
                    (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo
                    && r.StatusRezervacije != StatusRezervacije.Otkazana
                )
                .SumAsync(r => r.UkupnaCijena);
        }

        public async Task<Dictionary<DateTime, int>> DohvatiRezervacijePoDanimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            var rezervacije = await _kontekst
                .Rezervacije.Where(r =>
                    (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo
                    && r.StatusRezervacije != StatusRezervacije.Otkazana
                )
                .GroupBy(r => r.PocetakRezervacije.Date)
                .Select(g => new { Datum = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(k => k.Datum, k => k.Broj);

            // Popuni dane bez rezervacija
            for (var dan = od.Date; dan <= doo.Date; dan = dan.AddDays(1))
            {
                if (!rezervacije.ContainsKey(dan))
                    rezervacije[dan] = 0;
            }

            return rezervacije.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<Dictionary<DateTime, decimal>> DohvatiPrihodePoDanimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            var prihodi = await _kontekst
                .Rezervacije.Where(r =>
                    (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo
                    && r.StatusRezervacije != StatusRezervacije.Otkazana
                )
                .GroupBy(r => r.PocetakRezervacije.Date)
                .Select(g => new { Datum = g.Key, Prihod = g.Sum(r => r.UkupnaCijena) })
                .ToDictionaryAsync(k => k.Datum, k => k.Prihod);

            for (var dan = od.Date; dan <= doo.Date; dan = dan.AddDays(1))
            {
                if (!prihodi.ContainsKey(dan))
                    prihodi[dan] = 0;
            }

            return prihodi.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<Dictionary<int, int>> DohvatiRezervacijePoSatimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            var rezervacije = await _kontekst
                .Rezervacije.Where(r =>
                    (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo
                )
                .ToListAsync();

            var rezultat = new Dictionary<int, int>();
            for (int i = 0; i < 24; i++)
            {
                rezultat[i] = rezervacije.Count(r => r.PocetakRezervacije.Hour == i);
            }

            return rezultat;
        }

        public async Task<Dictionary<DayOfWeek, int>> DohvatiRezervacijePoDanimaSedmiceAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            var rezervacije = await _kontekst
                .Rezervacije.Where(r =>
                    (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo
                )
                .ToListAsync();

            var rezultat = new Dictionary<DayOfWeek, int>();

            foreach (DayOfWeek dan in Enum.GetValues(typeof(DayOfWeek)))
            {
                rezultat[dan] = rezervacije.Count(r => r.PocetakRezervacije.DayOfWeek == dan);
            }

            return rezultat;
        }

        public async Task<Dictionary<DateTime, double>> DohvatiPopunjenostPoDanimaAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            int ukupnoMjesta = 0;
            if (parkingId == 0)
            {
                ukupnoMjesta = await _kontekst.Parkinzi.SumAsync(p => p.UkupnoMjesta);
            }
            else
            {
                var parking = await _kontekst.Parkinzi.FindAsync(parkingId);
                if (parking != null)
                {
                    ukupnoMjesta = parking.UkupnoMjesta;
                }
            }

            if (ukupnoMjesta == 0)
                return new Dictionary<DateTime, double>();

            var popunjenost = new Dictionary<DateTime, double>();
            var rezervacijePoDanima = await DohvatiRezervacijePoDanimaAsync(parkingId, od, doo);

            foreach (var dan in rezervacijePoDanima)
            {
                var popunjenostProcenat = (double)dan.Value / ukupnoMjesta * 100;
                popunjenost[dan.Key] = Math.Min(popunjenostProcenat, 100);
            }

            return popunjenost;
        }

        public async Task<double> DohvatiProsjecnuPopunjenostAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            var popunjenost = await DohvatiPopunjenostPoDanimaAsync(parkingId, od, doo);
            return popunjenost.Any() ? popunjenost.Values.Average() : 0;
        }

        public async Task<double> DohvatiMaksimalnuPopunjenostAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            var popunjenost = await DohvatiPopunjenostPoDanimaAsync(parkingId, od, doo);
            return popunjenost.Any() ? popunjenost.Values.Max() : 0;
        }

        public async Task<double> DohvatiMinimalnuPopunjenostAsync(
            int parkingId,
            DateTime od,
            DateTime doo
        )
        {
            var popunjenost = await DohvatiPopunjenostPoDanimaAsync(parkingId, od, doo);
            return popunjenost.Any() ? popunjenost.Values.Min() : 0;
        }

        public async Task<bool> PostojiLiAsync(int id)
        {
            return await _skup.AnyAsync(i => i.IzvjestajId == id);
        }

        public async Task<int> PrebrojAsync()
        {
            return await _skup.CountAsync();
        }

        public async Task<int> PrebrojPoParkinguAsync(int parkingId)
        {
            return await _skup.CountAsync(i => i.ParkingId == parkingId);
        }

        public async Task<int> DohvatiBrojKorisnikaZaPeriodAsync(int parkingId, DateTime od, DateTime doo)
        {
            return await _kontekst.Rezervacije
                .Where(r => (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo)
                .Select(r => r.KorisnikId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> DohvatiNoveKorisnikeZaPeriodAsync(DateTime od, DateTime doo)
        {
            return await _kontekst.Users
                .Where(u => u.DatumRegistracije >= od && u.DatumRegistracije <= doo)
                .CountAsync();
        }

        public async Task<Dictionary<DateTime, int>> DohvatiAktivneKorisnikePoDanimaAsync(int parkingId, DateTime od, DateTime doo)
        {
            var aktivni = await _kontekst.Rezervacije
                .Where(r => (parkingId == 0 || r.ParkingId == parkingId)
                    && r.PocetakRezervacije >= od
                    && r.PocetakRezervacije <= doo)
                .GroupBy(r => new { Datum = r.PocetakRezervacije.Date, KorisnikId = r.KorisnikId })
                .Select(g => g.Key.Datum)
                .GroupBy(d => d)
                .Select(g => new { Datum = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(k => k.Datum, k => k.Broj);

            for (var dan = od.Date; dan <= doo.Date; dan = dan.AddDays(1))
            {
                if (!aktivni.ContainsKey(dan))
                    aktivni[dan] = 0;
            }

            return aktivni.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<Dictionary<DateTime, int>> DohvatiNoveRegistracijePoDanimaAsync(DateTime od, DateTime doo)
        {
            var registracije = await _kontekst.Users
                .Where(u => u.DatumRegistracije >= od && u.DatumRegistracije <= doo)
                .GroupBy(u => u.DatumRegistracije.Date)
                .Select(g => new { Datum = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(k => k.Datum, k => k.Broj);

            for (var dan = od.Date; dan <= doo.Date; dan = dan.AddDays(1))
            {
                if (!registracije.ContainsKey(dan))
                    registracije[dan] = 0;
            }

            return registracije.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<IEnumerable<SelectListItem>> DohvatiSveParkingeZaSelectListAsync()
        {
            return await _kontekst
                .Parkinzi.Select(p => new SelectListItem
                {
                    Value = p.ParkingId.ToString(),
                    Text = $"{p.Naziv} - {p.Adresa}",
                })
                .ToListAsync();
        }
    }
}
