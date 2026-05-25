using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Parking.Admin;
using smartPark.Models.ViewModels.Parking.Menadzer;
using smartPark.Repositories.Interfaces;

namespace smartPark.Repositories.Implementations
{
    public class ParkingRepository : IParkingRepository
    {
        private readonly ApplicationDbContext _kontekst;
        private readonly DbSet<Parking> _skup;

        public ParkingRepository(ApplicationDbContext kontekst)
        {
            _kontekst = kontekst;
            _skup = kontekst.Parkinzi;
        }

        public async Task<Parking?> DohvatiPoIdAsync(int id)
        {
            return await _skup.Include(p => p.Menadzer).FirstOrDefaultAsync(p => p.ParkingId == id);
        }

        public async Task<Parking?> DohvatiPoIdSaRezervacijamaAsync(int id)
        {
            return await _skup
                .Include(p => p.Menadzer)
                .Include(p => p.Rezervacije)
                .FirstOrDefaultAsync(p => p.ParkingId == id);
        }

        public async Task<Parking?> DohvatiPoIdSaParkingMjestimaAsync(int id)
        {
            return await _skup
                .Include(p => p.Menadzer)
                .Include(p => p.ParkingMjesta)
                .FirstOrDefaultAsync(p => p.ParkingId == id);
        }

        public async Task<IEnumerable<Parking>> DohvatiSveAsync()
        {
            return await _skup.Include(p => p.Menadzer).OrderBy(p => p.Naziv).ToListAsync();
        }

        public async Task<IEnumerable<Parking>> DohvatiSveSaMenadzerimaAsync()
        {
            return await _skup
                .Include(p => p.Menadzer)
                .Where(p => p.Menadzer != null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Parking>> DohvatiAktivneAsync()
        {
            return await _skup.Include(p => p.Menadzer).Where(p => p.Aktivan).ToListAsync();
        }

        public async Task<IEnumerable<Parking>> PronadjiAsync(Expression<Func<Parking, bool>> uslov)
        {
            return await _skup.Include(p => p.Menadzer).Where(uslov).ToListAsync();
        }

        public async Task DodajAsync(Parking parking)
        {
            await _skup.AddAsync(parking);
        }

        public void Izmijeni(Parking parking)
        {
            _skup.Update(parking);
        }

        public void Obrisi(Parking parking)
        {
            _skup.Remove(parking);
        }

        public async Task SacuvajPromjeneAsync()
        {
            await _kontekst.SaveChangesAsync();
        }

        public async Task<Parking?> DohvatiParkingPoMenadzeruAsync(string menadzerId)
        {
            return await _skup
                .Include(p => p.ParkingMjesta)
                .Include(p => p.Rezervacije)
                .FirstOrDefaultAsync(p => p.MenadzerID == menadzerId);
        }

        public async Task<List<Parking>> DohvatiSveParkingePoMenadzeruAsync(string menadzerId)
        {
            return await _skup
                .Include(p => p.ParkingMjesta)
                .Include(p => p.Rezervacije)
                .Where(p => p.MenadzerID == menadzerId)
                .ToListAsync();
        }

        public async Task<bool> DaLiMenadzerUpravljaParkingomAsync(string menadzerId, int parkingId)
        {
            return await _skup.AnyAsync(p =>
                p.ParkingId == parkingId && p.MenadzerID == menadzerId
            );
        }

        public async Task<int> DohvatiBrojRezervacijaZaParkingAsync(
            int parkingId,
            DateTime? od = null,
            DateTime? doo = null
        )
        {
            var upit = _kontekst.Rezervacije.Where(r => r.ParkingId == parkingId);

            if (od.HasValue)
                upit = upit.Where(r => r.PocetakRezervacije >= od.Value);
            if (doo.HasValue)
                upit = upit.Where(r => r.KrajRezervacije <= doo.Value);

            return await upit.CountAsync();
        }

        public async Task<decimal> DohvatiPrihodZaParkingAsync(
            int parkingId,
            DateTime? od = null,
            DateTime? doo = null
        )
        {
            var upit = _kontekst.Rezervacije.Where(r => r.ParkingId == parkingId);

            if (od.HasValue)
                upit = upit.Where(r => r.PocetakRezervacije >= od.Value);
            if (doo.HasValue)
                upit = upit.Where(r => r.KrajRezervacije <= doo.Value);

            return await upit.SumAsync(r => r.UkupnaCijena);
        }

        public async Task<double> DohvatiProsjecnuZauzetostAsync(
            int parkingId,
            DateTime? od = null,
            DateTime? doo = null
        )
        {
            var parking = await DohvatiPoIdAsync(parkingId);
            if (parking == null)
                return 0;

            var pocetak = od ?? DateTime.Now.AddDays(-30);
            var kraj = doo ?? DateTime.Now;

            var brojRezervacija = await DohvatiBrojRezervacijaZaParkingAsync(
                parkingId,
                pocetak,
                kraj
            );

            var brojDana = (kraj.Date - pocetak.Date).Days + 1;
            if (brojDana <= 0)
                return 0;

            var prosjecanBrojRezervacijaPoDanu = (double)brojRezervacija / brojDana;
            var zauzetost = (prosjecanBrojRezervacijaPoDanu / parking.UkupnoMjesta) * 100;

            return Math.Min(zauzetost, 100);
        }

        public async Task<Dictionary<int, int>> DohvatiRezervacijePoSatimaAsync(
            int parkingId,
            DateTime? datum = null
        )
        {
            var targetDate = datum ?? DateTime.Now.Date;
            var rezervacije = await _kontekst
                .Rezervacije.Where(r =>
                    r.ParkingId == parkingId && r.PocetakRezervacije.Date == targetDate
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
            int parkingId
        )
        {
            var rezervacije = await _kontekst
                .Rezervacije.Where(r => r.ParkingId == parkingId)
                .ToListAsync();

            var rezultat = new Dictionary<DayOfWeek, int>();

            foreach (DayOfWeek dan in Enum.GetValues(typeof(DayOfWeek)))
            {
                rezultat[dan] = rezervacije.Count(r => r.PocetakRezervacije.DayOfWeek == dan);
            }

            return rezultat;
        }

        // Za admina

        public async Task<int> DohvatiUkupnoParkingaAsync()
        {
            return await _skup.CountAsync();
        }

        public async Task<int> DohvatiBrojAktivnihParkingaAsync()
        {
            return await _skup.CountAsync(p => p.Aktivan);
        }

        public async Task<int> DohvatiUkupnoMjestaAsync()
        {
            return await _skup.SumAsync(p => p.UkupnoMjesta);
        }

        public async Task<int> DohvatiUkupnoSlobodnihMjestaAsync()
        {
            return await _skup.SumAsync(p => p.SlobodnaMjesta);
        }

        public async Task<decimal> DohvatiUkupniPrihodZaPeriodAsync(DateTime od, DateTime doo)
        {
            return await _kontekst
                .Rezervacije.Where(r => r.PocetakRezervacije >= od && r.KrajRezervacije <= doo)
                .SumAsync(r => r.UkupnaCijena);
        }

        public async Task<
            List<AdminParkingStatistikaViewModel.NajpopularnijiParking>
        > DohvatiNajpopularnijeParkingeAsync(int broj = 5)
        {
            var parkinzi = await _skup
                .Take(broj)
                .Select(p => new AdminParkingStatistikaViewModel.NajpopularnijiParking
                {
                    ParkingId = p.ParkingId,
                    Naziv = p.Naziv,
                    BrojRezervacija = _kontekst.Rezervacije.Count(r => r.ParkingId == p.ParkingId),
                    Prihod = _kontekst
                        .Rezervacije.Where(r => r.ParkingId == p.ParkingId)
                        .Sum(r => r.UkupnaCijena),
                    ProsjecnaZauzetost =
                        p.UkupnoMjesta > 0
                            ? (double)(p.UkupnoMjesta - p.SlobodnaMjesta) / p.UkupnoMjesta * 100
                            : 0,
                })
                .OrderByDescending(p => p.BrojRezervacija)
                .ToListAsync();

            return parkinzi;
        }

        public async Task<Dictionary<DateTime, int>> DohvatiRezervacijePoDanimaAsync(
            DateTime? od = null,
            DateTime? doo = null
        )
        {
            var pocetak = od ?? DateTime.Now.AddDays(-30);
            var kraj = doo ?? DateTime.Now;

            var rezervacije = await _kontekst
                .Rezervacije.Where(r =>
                    r.PocetakRezervacije >= pocetak && r.PocetakRezervacije <= kraj
                )
                .GroupBy(r => r.PocetakRezervacije.Date)
                .Select(g => new { Datum = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(k => k.Datum, k => k.Broj);

            // Popuni dane bez rezervacija
            for (var dan = pocetak.Date; dan <= kraj.Date; dan = dan.AddDays(1))
            {
                if (!rezervacije.ContainsKey(dan))
                {
                    rezervacije[dan] = 0;
                }
            }

            return rezervacije.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<Dictionary<DateTime, decimal>> DohvatiPrihodePoDanimaAsync(
            DateTime? od = null,
            DateTime? doo = null
        )
        {
            var pocetak = od ?? DateTime.Now.AddDays(-30);
            var kraj = doo ?? DateTime.Now;

            var prihodi = await _kontekst
                .Rezervacije.Where(r =>
                    r.PocetakRezervacije >= pocetak && r.PocetakRezervacije <= kraj
                )
                .GroupBy(r => r.PocetakRezervacije.Date)
                .Select(g => new { Datum = g.Key, Prihod = g.Sum(r => r.UkupnaCijena) })
                .ToDictionaryAsync(k => k.Datum, k => k.Prihod);

            // Popuni dane bez prihoda
            for (var dan = pocetak.Date; dan <= kraj.Date; dan = dan.AddDays(1))
            {
                if (!prihodi.ContainsKey(dan))
                {
                    prihodi[dan] = 0;
                }
            }

            return prihodi.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        public async Task<
            List<MenadzerParkingDetaljiViewModel.AktivnaRezervacija>
        > DohvatiAktivneRezervacijeZaParkingAsync(int parkingId)
        {
            var aktivneRezervacije = await _kontekst
                .Rezervacije.Include(r => r.Korisnik)
                .Include(r => r.ParkingMjesto)
                .Where(r =>
                    r.ParkingId == parkingId
                    && r.StatusRezervacije == StatusRezervacije.Aktivna
                    && r.PocetakRezervacije <= DateTime.Now
                    && r.KrajRezervacije >= DateTime.Now
                )
                .Select(r => new MenadzerParkingDetaljiViewModel.AktivnaRezervacija
                {
                    RezervacijaId = r.RezervacijaId,
                    KorisnikIme = r.Korisnik.Ime,
                    KorisnikPrezime = r.Korisnik.Prezime,
                    KorisnikEmail = r.Korisnik.Email ?? string.Empty,
                    Pocetak = r.PocetakRezervacije,
                    Kraj = r.KrajRezervacije,
                    BrojVozacke = r.Korisnik.BrojVozacke ?? string.Empty,
                    ParkingMjestoBroj = r.ParkingMjesto != null ? r.ParkingMjesto.BrojMjesta : 0,
                })
                .ToListAsync();

            return aktivneRezervacije;
        }

        public async Task<int> DohvatiBrojAktivnihRezervacijaTrenutnoAsync(int parkingId)
        {
            var sada = DateTime.Now;
            return await _kontekst.Rezervacije.CountAsync(r =>
                r.ParkingId == parkingId
                && r.StatusRezervacije == StatusRezervacije.Aktivna
                && r.PocetakRezervacije <= sada
                && r.KrajRezervacije >= sada
            );
        }

        public async Task<bool> PostojiLiAsync(int id)
        {
            return await _skup.AnyAsync(p => p.ParkingId == id);
        }

        public async Task<bool> PostojiLiNazivAsync(string naziv, int? izuzmiId = null)
        {
            if (izuzmiId.HasValue)
            {
                return await _skup.AnyAsync(p =>
                    p.Naziv.ToLower() == naziv.ToLower() && p.ParkingId != izuzmiId.Value
                );
            }
            return await _skup.AnyAsync(p => p.Naziv.ToLower() == naziv.ToLower());
        }

        public async Task<int> PrebrojAsync()
        {
            return await _skup.CountAsync();
        }

        public async Task<Dictionary<TipParkinga, int>> DohvatiBrojParkingaPoTipuAsync()
        {
            var rezultat = await _skup
                .GroupBy(p => p.TipParkinga)
                .Select(g => new { Tip = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(k => k.Tip, k => k.Broj);

            return rezultat;
        }

        public async Task<Dictionary<TipParkinga, decimal>> DohvatiProsjecnuCijenuPoTipuAsync()
        {
            var rezultat = await _skup
                .GroupBy(p => p.TipParkinga)
                .Select(g => new { Tip = g.Key, ProsjecnaCijena = g.Average(p => p.CijenaPoSatu) })
                .ToDictionaryAsync(k => k.Tip, k => k.ProsjecnaCijena);

            return rezultat;
        }

        public async Task<IEnumerable<SelectListItem>> DohvatiSveMenadzereZaSelectListAsync()
        {
            var menadzerRole = await _kontekst.Roles.FirstOrDefaultAsync(r => r.Name == "Menadzer");
            if (menadzerRole == null)
            {
                return new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Bez menadžera --" } };
            }

            var menadzeri = await _kontekst.Users
                .Where(k => _kontekst.UserRoles.Any(ur => ur.UserId == k.Id && ur.RoleId == menadzerRole.Id))
                .Select(k => new SelectListItem
                {
                    Value = k.Id,
                    Text = $"{k.Ime} {k.Prezime} - {k.Email}",
                })
                .ToListAsync();

            menadzeri.Insert(0, new SelectListItem { Value = "", Text = "-- Bez menadžera --" });

            return menadzeri;
        } 

        public async Task<int> PrebrojRezervacijeAsync()
        {
            return await _kontekst.Rezervacije.CountAsync();
        }

        public async Task<decimal> DohvatiUkupniPrihodAsync()
        {
            return await _kontekst
                .Rezervacije.Where(r => r.StatusRezervacije == StatusRezervacije.Zavrsena)
                .SumAsync(r => r.UkupnaCijena);
        }

        public async Task<List<Rezervacija>> DohvatiPosljednjeRezervacijeKorisnikaAsync(
            string korisnikId,
            int broj
        )
        {
            return await _kontekst
                .Rezervacije.Include(r => r.Parking)
                .Where(r => r.KorisnikId == korisnikId)
                .OrderByDescending(r => r.DatumKreiranjaRezervacije)
                .Take(broj)
                .ToListAsync();
        }
    }
}
