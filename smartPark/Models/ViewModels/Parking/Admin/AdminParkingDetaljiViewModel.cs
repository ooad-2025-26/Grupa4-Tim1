using smartPark.Models.ViewModels.Parking.Shared;

namespace smartPark.Models.ViewModels.Parking.Admin
{
    public class AdminParkingDetaljiViewModel : ParkingOsnovniViewModel
    {
        public string? MenadzerId { get; set; }
        public string? MenadzerIme { get; set; }
        public string? MenadzerPrezime { get; set; }
        public string MenadzerEmail { get; set; } = string.Empty;
        public DateTime DatumKreiranja { get; set; }

        // Statistika
        public int BrojRezervacijaDanas { get; set; }
        public int BrojRezervacijaSedmica { get; set; }
        public int BrojRezervacijaMjesec { get; set; }
        public decimal PrihodDanas { get; set; }
        public decimal PrihodSedmica { get; set; }
        public decimal PrihodMjesec { get; set; }
        public int BrojParkingMjesta { get; set; }
        public int BrojZauzetihMjesta => UkupnoMjesta - SlobodnaMjesta;

        // Liste
        public List<RezervacijaInfo> PosljednjeRezervacije { get; set; } = new();
        public List<ParkingMjestoInfo> ParkingMjestaInfo { get; set; } = new();

        public class RezervacijaInfo
        {
            public int RezervacijaId { get; set; }
            public string KorisnikIme { get; set; } = null!;
            public string KorisnikPrezime { get; set; } = null!;
            public DateTime Pocetak { get; set; }
            public DateTime Kraj { get; set; }
            public decimal Cijena { get; set; }
            public string Status { get; set; } = null!;
        }

        public class ParkingMjestoInfo
        {
            public int ParkingMjestoId { get; set; }
            public int BrojMjesta { get; set; }
            public string Zona { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string? TrenutniKorisnik { get; set; }
        }
    }
}
