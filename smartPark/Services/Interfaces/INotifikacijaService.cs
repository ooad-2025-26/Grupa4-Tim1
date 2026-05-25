using smartPark.Models.ViewModels.Notifikacija;

namespace smartPark.Services.Interfaces
{
    public interface INotifikacijaService
    {
        Task<bool> PosaljiEmailAsync(NotifikacijaPosaljiViewModel model);

        Task<bool> PosaljiTemplateEmailAsync(
            NotifikacijaTemplateViewModel template,
            string emailPrimaoca
        );

        Task<bool> PosaljiPotvrduRezervacijeAsync(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            decimal cijena
        );

        Task<bool> PosaljiObavjestenjeOtkazanoAsync(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            string razlog
        );

        Task<bool> PosaljiPodsetnikRezervacijeAsync(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj
        );

        Task<bool> PosaljiDobrodoslicuAsync(string email, string ime, string prezime);

        Task<bool> PosaljiAdminObavjestenjeNoviKorisnikAsync(
            string ime,
            string prezime,
            string email
        );

        Task<bool> TestirajEmailKonfiguracijuAsync(string testEmail);
    }
}
