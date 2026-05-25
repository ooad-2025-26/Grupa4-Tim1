using smartPark.Models.ViewModels.Korisnik.Shared;

namespace smartPark.Models.ViewModels.Korisnik.Menadzer;

public class MenadzerZaposleniciViewModel
{
    public int ParkingId { get; set; }
    public string ParkingNaziv { get; set; } = string.Empty;
    public List<KorisnikListaStavkaViewModel> Zaposlenici { get; set; } = new();
    public int UkupnoZaposlenih { get; set; }
    public int AktivnihZaposlenih { get; set; }
    public string? Filter { get; set; }
}
