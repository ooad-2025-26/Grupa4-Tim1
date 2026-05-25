namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class PopunjenostIzvjestajViewModel
    {
        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public DateTime PeriodOd { get; set; }
        public DateTime PeriodDo { get; set; }

        public int UkupnoMjesta { get; set; }
        public int UkupnoRezervacija { get; set; }
        public double ProsjecnaPopunjenost { get; set; }
        public double MaksimalnaPopunjenost { get; set; }
        public double MinimalnaPopunjenost { get; set; }

        public List<PopunjenostDnevna> DnevnaPopunjenost { get; set; } = new();

        public class PopunjenostDnevna
        {
            public DateTime Datum { get; set; }
            public int BrojZauzetihMjesta { get; set; }
            public int BrojSlobodnihMjesta { get; set; }
            public double PopunjenostProcenat { get; set; }
        }
    }
}
