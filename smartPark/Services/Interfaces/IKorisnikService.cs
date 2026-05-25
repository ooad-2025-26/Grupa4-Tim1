using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.ViewModels.Korisnik.Admin;
using smartPark.Models.ViewModels.Korisnik.Menadzer;
using smartPark.Models.ViewModels.Korisnik.Vozac;

namespace smartPark.Services.Interfaces
{
    public interface IKorisnikService
    {
        string DohvatiTrenutnogKorisnikaId(ClaimsPrincipal user);

        Task<AdminKorisnikListaViewModel> DohvatiAdminListuKorisnikaAsync(
            string? filterUloga = null,
            string? filterStatus = null,
            string? pretraga = null
        );
        Task<AdminKorisnikDetaljiViewModel?> DohvatiAdminDetaljeKorisnikaAsync(string id);
        Task<AdminKorisnikKreirajViewModel> DohvatiAdminViewModelZaKreiranjeAsync();
        Task<AdminKorisnikUrediViewModel?> DohvatiAdminViewModelZaUredjivanjeAsync(string id);
        Task<AdminStatistikaViewModel> DohvatiAdminStatistikuAsync();

        Task<(bool Uspjeh, string[] Greske)> AdminKreirajKorisnikaAsync(
            AdminKorisnikKreirajViewModel model
        );
        Task<(bool Uspjeh, string[] Greske)> AdminAzurirajKorisnikaAsync(
            AdminKorisnikUrediViewModel model
        );

        Task<MenadzerZaposleniciViewModel> DohvatiMenadzerZaposlenikeAsync(string menadzerId, string? filter = null);
        Task<MenadzerRadniciViewModel> DohvatiMenadzerRadnikeAsync(string menadzerId);

        Task<VozacProfilViewModel?> DohvatiVozacProfilAsync(string korisnikId);

        Task<string?> DohvatiUloguKorisnikaAsync(string korisnikId);
        Task<(bool Uspjeh, string Greska)> ZakljucajKorisnikaAsync(string id);
        Task<(bool Uspjeh, string Greska)> OtkljucajKorisnikaAsync(string id);
        Task<(bool Uspjeh, string Greska)> ObrisiKorisnikaAsync(string id);

        Task<IEnumerable<SelectListItem>> DohvatiSveUlogeZaSelectListAsync();
        Task<IEnumerable<SelectListItem>> DohvatiSveParkingeZaSelectListAsync();
        Task<int> DohvatiBrojRezervacijaKorisnikaAsync(string korisnikId);
        Task<int> DohvatiBrojAktivnihRezervacijaKorisnikaAsync(string korisnikId);
        Task<bool> EmailVecPostojiAsync(string email, string? izuzmiId = null);
    }
}
