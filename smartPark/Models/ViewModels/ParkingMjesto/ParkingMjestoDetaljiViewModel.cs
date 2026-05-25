namespace smartPark.Models.ViewModels.ParkingMjesto
{
    public class ParkingMjestoDetaljiViewModel : ParkingMjestoOsnovniViewModel
    {
        public string? TrenutnaRezervacijaId { get; set; }
        public string? TrenutniKorisnikIme { get; set; }
        public string? TrenutniKorisnikPrezime { get; set; }
        public DateTime? RezervacijaPocetak { get; set; }
        public DateTime? RezervacijaKraj { get; set; }

        // Informacije o parkingu
        public string ParkingAdresa { get; set; } = string.Empty;
        public decimal ParkingCijenaPoSatu { get; set; }
        public string ParkingTip { get; set; } = string.Empty;
    }
}
