using smartPark.Models.ViewModels.Parking.Shared;

namespace smartPark.Models.ViewModels.Parking.Menadzer
{
    public class MenadzerParkingDetaljiViewModel : ParkingOsnovniViewModel
    {
        public DateTime DatumKreiranja { get; set; }

        // Statistika za menadžera
        public int BrojRezervacijaDanas { get; set; }
        public int BrojRezervacijaSedmica { get; set; }
        public decimal PrihodDanas { get; set; }
        public decimal PrihodSedmica { get; set; }
        public int BrojAktivnihRezervacijaTrenutno { get; set; }

        // Liste
        public List<AktivnaRezervacija> AktivneRezervacije { get; set; } = new();
        public List<ParkingMjestoStatus> ParkingMjestaStatus { get; set; } = new();

        public class AktivnaRezervacija
        {
            public int RezervacijaId { get; set; }
            public string KorisnikIme { get; set; } = null!;
            public string KorisnikPrezime { get; set; } = null!;
            public string KorisnikEmail { get; set; } = null!;
            public DateTime Pocetak { get; set; }
            public DateTime Kraj { get; set; }
            public string BrojVozacke { get; set; } = null!;
            public int ParkingMjestoBroj { get; set; }
        }

        public class ParkingMjestoStatus
        {
            public int ParkingMjestoId { get; set; }
            public int BrojMjesta { get; set; }
            public string Zona { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string? ZauzeoKorisnik { get; set; }
            public DateTime? ZauzetoOd { get; set; }
        }
    }
}
