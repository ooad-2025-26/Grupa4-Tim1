using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Repositories.Interfaces;

namespace smartPark.Repositories.Implementations
{
    public class ParkingMjestoRepository : IParkingMjestoRepository
    {
        private readonly ApplicationDbContext _kontekst;
        private readonly DbSet<ParkingMjesto> _skup;

        public ParkingMjestoRepository(ApplicationDbContext kontekst)
        {
            _kontekst = kontekst;
            _skup = kontekst.ParkingMjesta;
        }

        public async Task<ParkingMjesto?> DohvatiPoIdAsync(int id)
        {
            return await _skup
                .Include(pm => pm.Parking)
                .FirstOrDefaultAsync(pm => pm.ParkingMjestoId == id);
        }

        public async Task<ParkingMjesto?> DohvatiPoIdSaRezervacijomAsync(int id)
        {
            var mjesto = await _skup
                .Include(pm => pm.Parking)
                .FirstOrDefaultAsync(pm => pm.ParkingMjestoId == id);

            if (mjesto != null)
            {
                mjesto.TrenutnaRezervacija = await _kontekst.Rezervacije
                    .Include(r => r.Korisnik)
                    .FirstOrDefaultAsync(r => r.ParkingMjestoId == id && 
                                              r.StatusRezervacije == StatusRezervacije.Aktivna &&
                                              r.PocetakRezervacije <= DateTime.Now &&
                                              r.KrajRezervacije >= DateTime.Now);
            }

            return mjesto;
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiSveAsync()
        {
            return await _skup
                .Include(pm => pm.Parking)
                .OrderBy(pm => pm.ParkingId)
                .ThenBy(pm => pm.BrojMjesta)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiSveSaParkingomAsync()
        {
            return await _skup.Include(pm => pm.Parking).ToListAsync();
        }

        public async Task<IEnumerable<ParkingMjesto>> PronadjiAsync(
            Expression<Func<ParkingMjesto, bool>> uslov
        )
        {
            return await _skup.Include(pm => pm.Parking).Where(uslov).ToListAsync();
        }

        public async Task DodajAsync(ParkingMjesto parkingMjesto)
        {
            await _skup.AddAsync(parkingMjesto);
        }

        public void Izmijeni(ParkingMjesto parkingMjesto)
        {
            _skup.Update(parkingMjesto);
        }

        public void Obrisi(ParkingMjesto parkingMjesto)
        {
            _skup.Remove(parkingMjesto);
        }

        public async Task SacuvajPromjeneAsync()
        {
            await _kontekst.SaveChangesAsync();
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiPoParkinguAsync(int parkingId)
        {
            return await _skup
                .Include(pm => pm.Parking)
                .Where(pm => pm.ParkingId == parkingId)
                .OrderBy(pm => pm.BrojMjesta)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiSlobodnaMjestaPoParkinguAsync(
            int parkingId
        )
        {
            return await _skup
                .Include(pm => pm.Parking)
                .Where(pm => pm.ParkingId == parkingId && pm.StatusMjesta == StatusMjesta.Slobodno)
                .OrderBy(pm => pm.BrojMjesta)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiZauzetaMjestaPoParkinguAsync(
            int parkingId
        )
        {
            return await _skup
                .Include(pm => pm.Parking)
                .Where(pm => pm.ParkingId == parkingId && pm.StatusMjesta == StatusMjesta.Zauzeto)
                .OrderBy(pm => pm.BrojMjesta)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParkingMjesto>> DohvatiRezervisanaMjestaPoParkinguAsync(
            int parkingId
        )
        {
            return await _skup
                .Include(pm => pm.Parking)
                .Where(pm => pm.ParkingId == parkingId && pm.StatusMjesta == StatusMjesta.Zauzeto)
                .OrderBy(pm => pm.BrojMjesta)
                .ToListAsync();
        }

        public async Task<ParkingMjesto?> DohvatiPrvoSlobodnoMjestoPoParkinguAsync(int parkingId)
        {
            return await _skup
                .Where(pm => pm.ParkingId == parkingId && pm.StatusMjesta == StatusMjesta.Slobodno)
                .OrderBy(pm => pm.BrojMjesta)
                .FirstOrDefaultAsync();
        }

        public async Task<int> DohvatiBrojSlobodnihMjestaPoParkinguAsync(int parkingId)
        {
            return await _skup.CountAsync(pm =>
                pm.ParkingId == parkingId && pm.StatusMjesta == StatusMjesta.Slobodno
            );
        }

        public async Task<int> DohvatiBrojZauzetihMjestaPoParkinguAsync(int parkingId)
        {
            return await _skup.CountAsync(pm =>
                pm.ParkingId == parkingId && pm.StatusMjesta == StatusMjesta.Zauzeto
            );
        }

        public async Task<int> DohvatiBrojRezervisanihMjestaPoParkinguAsync(int parkingId)
        {
            return await _skup.CountAsync(pm =>
                pm.ParkingId == parkingId && pm.StatusMjesta == StatusMjesta.Zauzeto
            );
        }

        public async Task<bool> AzurirajStatusAsync(int id, StatusMjesta noviStatus)
        {
            var mjesto = await DohvatiPoIdAsync(id);
            if (mjesto == null)
                return false;

            mjesto.StatusMjesta = noviStatus;
            Izmijeni(mjesto);
            await SacuvajPromjeneAsync();
            return true;
        }

        public async Task<bool> AzurirajStatusPoParkinguAsync(
            int parkingId,
            StatusMjesta noviStatus
        )
        {
            var mjesta = await DohvatiPoParkinguAsync(parkingId);
            foreach (var mjesto in mjesta)
            {
                mjesto.StatusMjesta = noviStatus;
            }
            await SacuvajPromjeneAsync();
            return true;
        }

        public async Task<bool> DodijeliRezervacijuMjestuAsync(
            int parkingMjestoId,
            int rezervacijaId
        )
        {
            var mjesto = await DohvatiPoIdAsync(parkingMjestoId);
            if (mjesto == null)
                return false;

            var rezervacija = await _kontekst.Rezervacije.FindAsync(rezervacijaId);
            if (rezervacija == null)
                return false;

            mjesto.StatusMjesta = StatusMjesta.Zauzeto;
            mjesto.TrenutnaRezervacija = rezervacija;
            
            rezervacija.ParkingMjestoId = parkingMjestoId;
            _kontekst.Rezervacije.Update(rezervacija);

            Izmijeni(mjesto);
            await SacuvajPromjeneAsync();
            return true;
        }

        public async Task<bool> OslobodiMjestoAsync(int parkingMjestoId)
        {
            var mjesto = await DohvatiPoIdAsync(parkingMjestoId);
            if (mjesto == null)
                return false;

            mjesto.StatusMjesta = StatusMjesta.Slobodno;
            mjesto.TrenutnaRezervacija = null;

            var rezervacija = await _kontekst.Rezervacije.FirstOrDefaultAsync(r => 
                r.ParkingMjestoId == parkingMjestoId && 
                r.StatusRezervacije == StatusRezervacije.Aktivna);
            if (rezervacija != null)
            {
                rezervacija.StatusRezervacije = StatusRezervacije.Zavrsena;
                _kontekst.Rezervacije.Update(rezervacija);
            }

            Izmijeni(mjesto);
            await SacuvajPromjeneAsync();
            return true;
        }

        public async Task<bool> PostojiLiAsync(int id)
        {
            return await _skup.AnyAsync(pm => pm.ParkingMjestoId == id);
        }

        public async Task<bool> PostojiLiBrojMjestaUParkinguAsync(
            int parkingId,
            int brojMjesta,
            int? izuzmiId = null
        )
        {
            if (izuzmiId.HasValue)
            {
                return await _skup.AnyAsync(pm =>
                    pm.ParkingId == parkingId
                    && pm.BrojMjesta == brojMjesta
                    && pm.ParkingMjestoId != izuzmiId.Value
                );
            }
            return await _skup.AnyAsync(pm =>
                pm.ParkingId == parkingId && pm.BrojMjesta == brojMjesta
            );
        }

        public async Task<int> PrebrojPoParkinguAsync(int parkingId)
        {
            return await _skup.CountAsync(pm => pm.ParkingId == parkingId);
        }

        public async Task<Dictionary<StatusMjesta, int>> DohvatiStatistikuPoParkinguAsync(
            int parkingId
        )
        {
            var statistika = new Dictionary<StatusMjesta, int>();

            foreach (StatusMjesta status in Enum.GetValues(typeof(StatusMjesta)))
            {
                var broj = await _skup.CountAsync(pm =>
                    pm.ParkingId == parkingId && pm.StatusMjesta == status
                );
                statistika[status] = broj;
            }

            return statistika;
        }

        public async Task<IEnumerable<SelectListItem>> DohvatiSveParkingeZaSelectListAsync()
        {
            var parkinzi = await _kontekst.Parkinzi.ToListAsync();
            return parkinzi.Select(p => new SelectListItem
            {
                Value = p.ParkingId.ToString(),
                Text = $"{p.Naziv} - {p.Adresa}",
            });
        }
    }
}
