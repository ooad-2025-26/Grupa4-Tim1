namespace smartPark.Models.ViewModels.Parking.Menadzer
{
    public class MenadzerParkingStatistikaViewModel
    {
        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = null!;

        // Dnevna statistika
        public int RezervacijaDanas { get; set; }
        public decimal PrihodDanas { get; set; }
        public double ProsjecnaZauzetostDanas { get; set; }

        // Sedmična statistika
        public int RezervacijaSedmica { get; set; }
        public decimal PrihodSedmica { get; set; }
        public double ProsjecnaZauzetostSedmica { get; set; }

        // Mjesečna statistika
        public int RezervacijaMjesec { get; set; }
        public decimal PrihodMjesec { get; set; }
        public double ProsjecnaZauzetostMjesec { get; set; }

        // Najprometniji sati
        public Dictionary<int, int> RezervacijePoSatima { get; set; } = new();

        // Statistika po danima u sedmici
        public Dictionary<DayOfWeek, int> RezervacijePoDanimaSedmice { get; set; } = new();

        // Grafikoni
        public Dictionary<DateTime, int> RezervacijeZadnjih7Dana { get; set; } = new();
        public Dictionary<DateTime, decimal> PrihodiZadnjih7Dana { get; set; } = new();
    }
}
