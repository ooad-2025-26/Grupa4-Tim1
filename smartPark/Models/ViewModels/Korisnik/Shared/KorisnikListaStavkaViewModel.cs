namespace smartPark.Models.ViewModels.Korisnik.Shared;

public class KorisnikListaStavkaViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Uloga { get; set; } = string.Empty;
    public bool Aktivan { get; set; }
    public DateTime DatumRegistracije { get; set; }
    public bool JeZakljucan { get; set; }
    public int BrojRezervacija { get; set; }
}
