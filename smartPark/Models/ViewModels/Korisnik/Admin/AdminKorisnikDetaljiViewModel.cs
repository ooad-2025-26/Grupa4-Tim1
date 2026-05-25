namespace smartPark.Models.ViewModels.Korisnik.Admin;

public class AdminKorisnikDetaljiViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Uloga { get; set; } = string.Empty;
    public bool Aktivan { get; set; }
    public DateTime DatumRegistracije { get; set; }
    public bool JeZakljucan { get; set; }
    public string? BrojVozacke { get; set; }
    public int? MenadzerOdgovorniParkingId { get; set; }
    public string? ParkingNaziv { get; set; }
    public List<MenadzerParkingInfo> OdgovorniParkinzi { get; set; } = new();

    public class MenadzerParkingInfo
    {
        public int ParkingId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Adresa { get; set; } = string.Empty;
    }
    public int BrojRezervacija { get; set; }
    public int BrojAktivnihRezervacija { get; set; }
    public int BrojNotifikacija { get; set; }
    public int BrojNecitanihNotifikacija { get; set; }
}
