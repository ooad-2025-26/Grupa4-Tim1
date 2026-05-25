using Microsoft.EntityFrameworkCore;
using smartPark.Data;
using smartPark.Models.Entities;
using smartPark.Repositories.Interfaces;

namespace smartPark.Repositories.Implementations
{
    public class QRKodRepository : IQRKodRepository
    {
        private readonly ApplicationDbContext _kontekst;
        private readonly DbSet<QRKod> _skup;

        public QRKodRepository(ApplicationDbContext kontekst)
        {
            _kontekst = kontekst;
            _skup = kontekst.QRKodovi;
        }

        public async Task<QRKod?> DohvatiPoIdAsync(int id)
        {
            return await _skup
                .Include(q => q.Rezervacija)
                    .ThenInclude(r => r.Parking)
                .Include(q => q.Rezervacija)
                    .ThenInclude(r => r.ParkingMjesto)
                .FirstOrDefaultAsync(q => q.QRKodId == id);
        }

        public async Task<QRKod?> DohvatiPoRezervacijiAsync(int rezervacijaId)
        {
            return await _skup
                .Include(q => q.Rezervacija)
                    .ThenInclude(r => r.Parking)
                .Include(q => q.Rezervacija)
                    .ThenInclude(r => r.ParkingMjesto)
                .FirstOrDefaultAsync(q => q.RezervacijaId == rezervacijaId);
        }

        public async Task<IEnumerable<QRKod>> DohvatiSveAsync()
        {
            return await _skup
                .Include(q => q.Rezervacija)
                .OrderByDescending(q => q.DatumGenerisanja)
                .ToListAsync();
        }

        public async Task DodajAsync(QRKod qrKod)
        {
            await _skup.AddAsync(qrKod);
        }

        public void Izmijeni(QRKod qrKod)
        {
            _skup.Update(qrKod);
        }

        public void Obrisi(QRKod qrKod)
        {
            _skup.Remove(qrKod);
        }

        public async Task SacuvajPromjeneAsync()
        {
            await _kontekst.SaveChangesAsync();
        }

        public async Task<bool> PostojiLiZaRezervacijuAsync(int rezervacijaId)
        {
            return await _skup.AnyAsync(q => q.RezervacijaId == rezervacijaId);
        }
    }
}
