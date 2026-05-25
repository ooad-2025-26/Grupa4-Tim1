namespace smartPark.Models.ViewModels.Korisnik.Shared;

public class KorisnikOsnovniViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PunoIme => $"{Ime} {Prezime}";
}
