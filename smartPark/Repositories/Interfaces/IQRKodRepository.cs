using smartPark.Models.Entities;

namespace smartPark.Repositories.Interfaces
{
    public interface IQRKodRepository
    {
        Task<QRKod?> DohvatiPoIdAsync(int id);
        Task<QRKod?> DohvatiPoRezervacijiAsync(int rezervacijaId);
        Task<IEnumerable<QRKod>> DohvatiSveAsync();
        Task DodajAsync(QRKod qrKod);
        void Izmijeni(QRKod qrKod);
        void Obrisi(QRKod qrKod);
        Task SacuvajPromjeneAsync();
        Task<bool> PostojiLiZaRezervacijuAsync(int rezervacijaId);
    }
}
