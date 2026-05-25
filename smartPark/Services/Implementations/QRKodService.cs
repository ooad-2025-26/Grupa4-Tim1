using smartPark.Helpers;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Rezervacija;
using smartPark.Repositories.Interfaces;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class QRKodService : IQRKodService
    {
        private readonly IQRKodRepository _qrKodRepository;
        private readonly IRezervacijaRepository _rezervacijaRepository;

        public QRKodService(
            IQRKodRepository qrKodRepository,
            IRezervacijaRepository rezervacijaRepository
        )
        {
            _qrKodRepository = qrKodRepository;
            _rezervacijaRepository = rezervacijaRepository;
        }

        public async Task<QRKodViewModel> GenerisiQRKodZaRezervacijuAsync(int rezervacijaId)
        {
            // Provjeri da li već postoji QR kod za ovu rezervaciju
            var postojeci = await _qrKodRepository.DohvatiPoRezervacijiAsync(rezervacijaId);
            if (postojeci != null)
            {
                return new QRKodViewModel
                {
                    QRKodId = postojeci.QRKodId,
                    Kod = postojeci.Kod,
                    Base64Slika = postojeci.Base64Slika,
                    DatumGenerisanja = postojeci.DatumGenerisanja,
                    DatumIsteka = postojeci.DatumIsteka,
                    Iskoristen = postojeci.Iskoristen,
                    RezervacijaId = postojeci.RezervacijaId,
                };
            }

            var rezervacija = await _rezervacijaRepository.DohvatiPoIdAsync(rezervacijaId);
            if (rezervacija == null)
                throw new KeyNotFoundException($"Rezervacija sa ID {rezervacijaId} nije pronađena");

            // Generiši jedinstveni kod
            var kod = GenerisiJedinstveniKod(rezervacija);

            // Generiši QR kod sliku
            var base64Slika = QRCodeGeneratorHelper.GenerisiQRKod(kod);

            var qrKod = new QRKod
            {
                Kod = kod,
                Base64Slika = base64Slika,
                DatumGenerisanja = DateTime.Now,
                DatumIsteka = rezervacija.KrajRezervacije.AddHours(2), // QR kod važi još 2h nakon kraja
                Iskoristen = false,
                RezervacijaId = rezervacijaId,
            };

            await _qrKodRepository.DodajAsync(qrKod);
            await _qrKodRepository.SacuvajPromjeneAsync();

            return new QRKodViewModel
            {
                QRKodId = qrKod.QRKodId,
                Kod = qrKod.Kod,
                Base64Slika = qrKod.Base64Slika,
                DatumGenerisanja = qrKod.DatumGenerisanja,
                DatumIsteka = qrKod.DatumIsteka,
                Iskoristen = qrKod.Iskoristen,
                RezervacijaId = qrKod.RezervacijaId,
            };
        }

        public async Task<QRKodViewModel?> DohvatiQRKodPoRezervacijiAsync(int rezervacijaId)
        {
            var qrKod = await _qrKodRepository.DohvatiPoRezervacijiAsync(rezervacijaId);
            if (qrKod == null)
                return null;

            return new QRKodViewModel
            {
                QRKodId = qrKod.QRKodId,
                Kod = qrKod.Kod,
                Base64Slika = qrKod.Base64Slika,
                DatumGenerisanja = qrKod.DatumGenerisanja,
                DatumIsteka = qrKod.DatumIsteka,
                Iskoristen = qrKod.Iskoristen,
                RezervacijaId = qrKod.RezervacijaId,
                ParkingNaziv = qrKod.Rezervacija?.Parking?.Naziv ?? string.Empty,
                ParkingMjestoBroj = qrKod.Rezervacija?.ParkingMjesto?.BrojMjesta.ToString()
            };
        }

        public async Task<bool> ValidirajQRKodAsync(string kod)
        {
            var qrKod = (await _qrKodRepository.DohvatiSveAsync()).FirstOrDefault(q =>
                q.Kod == kod
            );
            if (qrKod == null)
                return false;

            return !qrKod.Iskoristen && qrKod.DatumIsteka > DateTime.Now;
        }

        public async Task<smartPark.Models.Entities.QRKod?> DohvatiQRKodPoKoduAsync(string kod)
        {
            return (await _qrKodRepository.DohvatiSveAsync()).FirstOrDefault(q => q.Kod == kod);
        }

        public async Task<bool> IskoristiQRKodAsync(int rezervacijaId)
        {
            var qrKod = await _qrKodRepository.DohvatiPoRezervacijiAsync(rezervacijaId);
            if (qrKod == null)
                return false;

            if (qrKod.Iskoristen || qrKod.DatumIsteka < DateTime.Now)
                return false;

            qrKod.Iskoristen = true;
            _qrKodRepository.Izmijeni(qrKod);
            await _qrKodRepository.SacuvajPromjeneAsync();

            return true;
        }

        public async Task<bool> PonistiIskoristenjeAsync(int rezervacijaId)
        {
            var qrKod = await _qrKodRepository.DohvatiPoRezervacijiAsync(rezervacijaId);
            if (qrKod == null)
                return false;

            qrKod.Iskoristen = false;
            _qrKodRepository.Izmijeni(qrKod);
            await _qrKodRepository.SacuvajPromjeneAsync();

            return true;
        }

        private string GenerisiJedinstveniKod(Rezervacija rezervacija)
        {
            return $"SP-{rezervacija.RezervacijaId}-{rezervacija.KorisnikId}-{DateTime.Now.Ticks}";
        }
    }
}
