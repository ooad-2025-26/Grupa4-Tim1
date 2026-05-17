using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using smartPark.Models.Entities;
using smartPark.Models.ViewModels.Korisnik.Admin;

namespace smartPark.Repositories.Interfaces
{
    public interface IKorisnikRepository
    {
        Task<Korisnik?> DohvatiPoIdAsync(string id);
        Task<Korisnik?> DohvatiPoEmailAsync(string email);
        Task<IEnumerable<Korisnik>> DohvatiSveAsync();
        Task<IEnumerable<Korisnik>> PronadjiAsync(Expression<Func<Korisnik, bool>> uslov);
        Task<IEnumerable<Korisnik>> DohvatiSveSaRezervacijamaAsync();
        Task<IEnumerable<Korisnik>> DohvatiSveSaNotifikacijamaAsync();

        Task<IdentityResult> DodajAsync(Korisnik korisnik, string lozinka);
        Task<IdentityResult> AzurirajAsync(Korisnik korisnik);
        Task<IdentityResult> ObrisiAsync(Korisnik korisnik);

        Task<IEnumerable<string>> DohvatiSveRoleAsync();
        Task<IEnumerable<string>> DohvatiRoleKorisnikaAsync(Korisnik korisnik);
        Task<IdentityResult> DodajUloguKorisnikuAsync(Korisnik korisnik, string uloga);
        Task<IdentityResult> UkloniUloguKorisnikuAsync(Korisnik korisnik, string uloga);
        Task<bool> JeLiKorisnikUUloziAsync(Korisnik korisnik, string uloga);
        Task<IEnumerable<Korisnik>> DohvatiKorisnikePoUloziAsync(string uloga);
        Task<Dictionary<string, int>> DohvatiBrojKorisnikaPoUlogamaAsync();

        Task<IdentityResult> ZakljucajKorisnikaAsync(string id);
        Task<IdentityResult> OtkljucajKorisnikaAsync(string id);
        Task<bool> JeLiKorisnikZakljucanAsync(string id);

        Task<IEnumerable<Korisnik>> DohvatiZaposlenikePoParkinguAsync(int parkingId);
        Task<int> DohvatiBrojAktivnihRadnikaDanasAsync(int parkingId);

        Task<int> DohvatiBrojRezervacijaKorisnikaAsync(string korisnikId);
        Task<int> DohvatiBrojAktivnihRezervacijaKorisnikaAsync(string korisnikId);
        Task<int> DohvatiBrojNotifikacijaKorisnikaAsync(string korisnikId);
        Task<int> DohvatiBrojNecitanihNotifikacijaKorisnikaAsync(string korisnikId);

        Task<bool> PostojiLiSaEmailomAsync(string email, string? izuzmiId = null);
        Task<int> PrebrojAsync();
        Task<int> PrebrojPoUloziAsync(string uloga);
        Task<int> PrebrojAktivneAsync();
        Task<int> PrebrojNeaktivneAsync();
        Task<int> PrebrojZakljucaneAsync();

        Task<
            IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        > DohvatiSveUlogeZaSelectListAsync();
        Task<
            IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        > DohvatiSveParkingeZaSelectListAsync();

        Task<Dictionary<DateTime, int>> DohvatiRegistracijePoDanimaAsync(int brojDana = 30);
        Task<List<AdminStatistikaViewModel.NedavnaAktivnost>> DohvatiNedavneAktivnostiAsync(
            int broj = 10
        );
    }
}
