using smartPark.Models.ViewModels.Korisnik.Shared;

namespace smartPark.Models.ViewModels.Korisnik.Vozac
{
    public class VozacProfilViewModel : KorisnikOsnovniViewModel
    {
        public string? BrojVozacke { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public int BrojRezervacija { get; set; }
        public int BrojAktivnihRezervacija { get; set; }
        public int BrojNotifikacija { get; set; }
        public int BrojNecitanihNotifikacija { get; set; }

        // Posljednje rezervacije
        public List<PosljednjaRezervacija> PosljednjeRezervacije { get; set; } = new();

        public class PosljednjaRezervacija
        {
            public int RezervacijaId { get; set; }
            public string ParkingNaziv { get; set; } = null!;
            public DateTime DatumPocetka { get; set; }
            public DateTime DatumKraja { get; set; }
            public string Status { get; set; } = null!;
            public decimal Cijena { get; set; }
        }
    }
}
