namespace smartPark.Models.ViewModels.ParkingMjesto
{
    public class ParkingMjestoListaViewModel
    {
        public List<ParkingMjestoOsnovniViewModel> ParkingMjesta { get; set; } = new();
        public int UkupnoMjesta { get; set; }
        public int SlobodnihMjesta { get; set; }
        public int ZauzetihMjesta { get; set; }
        public int RezervisanihMjesta { get; set; }
        public int NedostupnihMjesta { get; set; }

        public int? ParkingFilter { get; set; }
        public string? StatusFilter { get; set; }

        public double ProcenatZauzetosti =>
            UkupnoMjesta > 0 ? (double)ZauzetihMjesta / UkupnoMjesta * 100 : 0;
    }
}
