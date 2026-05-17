using smartPark.Models.ViewModels.Korisnik.Shared;

namespace smartPark.Models.ViewModels.Korisnik.Admin
{
    public class AdminKorisnikDetaljiViewModel : KorisnikOsnovniViewModel
    {
        public string Uloga { get; set; } = null!;
        public bool Aktivan { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public DateTime? DatumZadnjePrijave { get; set; }
        public bool JeZakljucan { get; set; }
        public string? BrojVozacke { get; set; }
        public int? MenadzerOdgovorniParkingId { get; set; }
        public string? ParkingNaziv { get; set; }
        public int BrojRezervacija { get; set; }
        public int BrojAktivnihRezervacija { get; set; }
        public int BrojNotifikacija { get; set; }
        public int BrojNecitanihNotifikacija { get; set; }

        // Za prikaz
        public string Status => Aktivan ? "Aktivan" : "Neaktivan";
        public string UlogaBoja =>
            Uloga switch
            {
                "Administrator" => "danger",
                "Menadzer" => "warning",
                "Vozac" => "success",
                _ => "secondary",
            };
    }
}
