namespace smartPark.Models.ViewModels.Parking.Shared
{
    public class ParkingListaStavkaViewModel : ParkingOsnovniViewModel
    {
        public string? MenadzerIme { get; set; }
        public string? MenadzerPrezime { get; set; }
        public string MenadzerPunoIme =>
            !string.IsNullOrEmpty(MenadzerIme)
                ? $"{MenadzerIme} {MenadzerPrezime}"
                : "Nema menadžera";
        public int BrojAktivnihRezervacija { get; set; }
        public decimal DnevniPrihod { get; set; }
    }
}
