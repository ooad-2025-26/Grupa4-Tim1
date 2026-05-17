namespace smartPark.Models.ViewModels.Parking.Admin
{
    public class AdminParkingStatistikaViewModel
    {
        // Osnovna statistika
        public int UkupnoParkinga { get; set; }
        public int AktivnihParkinga { get; set; }
        public int NeaktivnihParkinga { get; set; }
        public int UkupnoMjesta { get; set; }
        public int UkupnoSlobodnihMjesta { get; set; }
        public decimal UkupniPrihodDanas { get; set; }
        public decimal UkupniPrihodSedmica { get; set; }
        public decimal UkupniPrihodMjesec { get; set; }
        public decimal UkupniPrihodGodina { get; set; }

        // Statistika po tipu parkinga
        public int BrojOtvorenih { get; set; }
        public int BrojZatvorenih { get; set; }
        public decimal ProsjecnaCijenaOtvorenih { get; set; }
        public decimal ProsjecnaCijenaZatvorenih { get; set; }

        // Najpopularniji parkinzi
        public List<NajpopularnijiParking> NajpopularnijiParkinzi { get; set; } = new();

        // Statistika po danima (za grafikon)
        public Dictionary<DateTime, int> RezervacijePoDanima { get; set; } = new();
        public Dictionary<DateTime, decimal> PrihodiPoDanima { get; set; } = new();

        public class NajpopularnijiParking
        {
            public int ParkingId { get; set; }
            public string Naziv { get; set; } = null!;
            public int BrojRezervacija { get; set; }
            public decimal Prihod { get; set; }
            public double ProsjecnaZauzetost { get; set; }
        }
    }
}
