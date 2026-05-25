namespace smartPark.Models.ViewModels.Rezervacija
{
    public class RezervacijaListaViewModel
    {
        public List<RezervacijaOsnovniViewModel> Rezervacije { get; set; } = new();
        public int UkupnoRezervacija { get; set; }
        public int AktivnihRezervacija { get; set; }
        public int OtkazanihRezervacija { get; set; }
        public int ZavrsenihRezervacija { get; set; }
        public int IsteklihRezervacija { get; set; }
        public decimal UkupniPrihod { get; set; }

        public int? ParkingFilter { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? DatumOd { get; set; }
        public DateTime? DatumDo { get; set; }

        public IEnumerable<string> DostupniStatusi { get; set; } = new List<string>();
    }
}
