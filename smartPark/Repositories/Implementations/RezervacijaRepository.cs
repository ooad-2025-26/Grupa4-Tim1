using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Repositories.Interfaces;

namespace smartPark.Repositories.Implementations
{
    public class RezervacijaRepository : IRezervacijaRepository
    {
        private readonly ApplicationDbContext _kontekst;
        private readonly DbSet<Rezervacija> _skup;

        public RezervacijaRepository(ApplicationDbContext kontekst)
        {
            _kontekst = kontekst;
            _skup = kontekst.Rezervacije;
        }

        public async Task<Rezervacija?> DohvatiPoIdAsync(int id)
        {
            return await _skup
                .Include(r => r.Korisnik)
                .Include(r => r.Parking)
                .Include(r => r.ParkingMjesto)
                .FirstOrDefaultAsync(r => r.RezervacijaId == id);
        }

        public async Task<Rezervacija?> DohvatiPoIdSaSvimeAsync(int id)
        {
            return await _skup
                .Include(r => r.Korisnik)
                .Include(r => r.Parking)
                .Include(r => r.ParkingMjesto)
                .Include(r => r.QRKodRezervacije)
                .FirstOrDefaultAsync(r => r.RezervacijaId == id);
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiSveAsync()
        {
            return await _skup
                .Include(r => r.Korisnik)
                .Include(r => r.Parking)
                .Include(r => r.ParkingMjesto)
                .OrderByDescending(r => r.DatumKreiranjaRezervacije)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiSveSaSvimeAsync()
        {
            return await _skup
                .Include(r => r.Korisnik)
                .Include(r => r.Parking)
                .Include(r => r.ParkingMjesto)
                .Include(r => r.QRKodRezervacije)
                .OrderByDescending(r => r.DatumKreiranjaRezervacije)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rezervacija>> PronadjiAsync(
            Expression<Func<Rezervacija, bool>> uslov
        )
        {
            return await _skup
                .Include(r => r.Korisnik)
                .Include(r => r.Parking)
                .Include(r => r.ParkingMjesto)
                .Where(uslov)
                .ToListAsync();
        }

        public async Task DodajAsync(Rezervacija rezervacija)
        {
            await _skup.AddAsync(rezervacija);
        }

        public void Izmijeni(Rezervacija rezervacija)
        {
            _skup.Update(rezervacija);
        }

        public void Obrisi(Rezervacija rezervacija)
        {
            _skup.Remove(rezervacija);
        }

        public async Task SacuvajPromjeneAsync()
        {
            await _kontekst.SaveChangesAsync();
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiPoKorisnikuAsync(string korisnikId)
        {
            return await _skup
                .Include(r => r.Parking)
                .Include(r => r.ParkingMjesto)
                .Where(r => r.KorisnikId == korisnikId)
                .OrderByDescending(r => r.DatumKreiranjaRezervacije)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiPoParkinguAsync(int parkingId)
        {
            return await _skup
                .Include(r => r.Korisnik)
                .Where(r => r.ParkingId == parkingId)
                .OrderByDescending(r => r.DatumKreiranjaRezervacije)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiAktivneRezervacijeAsync()
        {
            var sada = DateTime.UtcNow;
            return await _skup
                .Include(r => r.Korisnik)
                .Include(r => r.Parking)
                .Where(r =>
                    r.StatusRezervacije == StatusRezervacije.Aktivna
                    && r.PocetakRezervacije <= sada
                    && r.KrajRezervacije >= sada
                )
                .ToListAsync();
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiAktivneRezervacijeZaParkingAsync(
            int parkingId
        )
        {
            var sada = DateTime.UtcNow;
            return await _skup
                .Include(r => r.Korisnik)
                .Where(r =>
                    r.ParkingId == parkingId
                    && r.StatusRezervacije == StatusRezervacije.Aktivna
                    && r.PocetakRezervacije <= sada
                    && r.KrajRezervacije >= sada
                )
                .ToListAsync();
        }

        public async Task<IEnumerable<Rezervacija>> DohvatiRezervacijeZaPeriodAsync(
            DateTime pocetak,
            DateTime kraj
        )
        {
            return await _skup
                .Include(r => r.Korisnik)
                .Include(r => r.Parking)
                .Where(r => r.PocetakRezervacije >= pocetak && r.KrajRezervacije <= kraj)
                .OrderBy(r => r.PocetakRezervacije)
                .ToListAsync();
        }

        public async Task<bool> PostojiLiPreklapanjeAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        )
        {
            var upit = _skup.Where(r =>
                r.ParkingId == parkingId
                && r.StatusRezervacije == StatusRezervacije.Aktivna
                && r.PocetakRezervacije < kraj
                && r.KrajRezervacije > pocetak
            );

            if (izuzmiId.HasValue)
                upit = upit.Where(r => r.RezervacijaId != izuzmiId.Value);

            return await upit.AnyAsync();
        }

        public async Task<bool> PostojiLiPreklapanjeZaMjestoAsync(
            int parkingMjestoId,
            DateTime pocetak,
            DateTime kraj,
            int? izuzmiId = null
        )
        {
            var upit = _skup.Where(r =>
                r.ParkingMjestoId == parkingMjestoId
                && r.StatusRezervacije == StatusRezervacije.Aktivna
                && r.PocetakRezervacije < kraj
                && r.KrajRezervacije > pocetak
            );

            if (izuzmiId.HasValue)
                upit = upit.Where(r => r.RezervacijaId != izuzmiId.Value);

            return await upit.AnyAsync();
        }

        public async Task<int> DohvatiBrojRezervacijaPoStatusuAsync(StatusRezervacije status)
        {
            return await _skup.CountAsync(r => r.StatusRezervacije == status);
        }

        public async Task<Dictionary<DateTime, int>> DohvatiRezervacijePoDanimaAsync(
            int brojDana = 30
        )
        {
            var pocetak = DateTime.UtcNow.AddDays(-brojDana).Date;
            var rezervacije = await _skup
                .Where(r => r.DatumKreiranjaRezervacije >= pocetak)
                .GroupBy(r => r.DatumKreiranjaRezervacije.Date)
                .Select(g => new { Datum = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(k => k.Datum, k => k.Broj);

            for (var dan = pocetak; dan <= DateTime.UtcNow.Date; dan = dan.AddDays(1))
            {
                if (!rezervacije.ContainsKey(dan))
                    rezervacije[dan] = 0;
            }

            return rezervacije.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<decimal> DohvatiUkupniPrihodZaPeriodAsync(DateTime od, DateTime doo)
        {
            return await _skup
                .Where(r =>
                    r.StatusRezervacije == StatusRezervacije.Zavrsena
                    && r.DatumKreiranjaRezervacije >= od
                    && r.DatumKreiranjaRezervacije <= doo
                )
                .SumAsync(r => r.UkupnaCijena);
        }

        public async Task<
            IEnumerable<SelectListItem>
        > DohvatiSlobodnaParkingMjestaZaSelectListAsync(
            int parkingId,
            DateTime pocetak,
            DateTime kraj
        )
        {
            // Dohvati sva parking mjesta koja nisu rezervisana u datom periodu
            var zauzetaMjesta = await _skup
                .Where(r =>
                    r.ParkingId == parkingId
                    && r.StatusRezervacije == StatusRezervacije.Aktivna
                    && r.PocetakRezervacije < kraj
                    && r.KrajRezervacije > pocetak
                )
                .Select(r => r.ParkingMjestoId)
                .ToListAsync();

            var slobodnaMjesta = await _kontekst
                .ParkingMjesta.Where(pm =>
                    pm.ParkingId == parkingId
                    && pm.StatusMjesta == StatusMjesta.Slobodno
                    && !zauzetaMjesta.Contains(pm.ParkingMjestoId)
                )
                .Select(pm => new SelectListItem
                {
                    Value = pm.ParkingMjestoId.ToString(),
                    Text = $"Mjesto {pm.BrojMjesta}",
                })
                .ToListAsync();

            slobodnaMjesta.Insert(
                0,
                new SelectListItem { Value = "", Text = "-- Automatski odaberi slobodno mjesto --" }
            );

            return slobodnaMjesta;
        }
    }
}
