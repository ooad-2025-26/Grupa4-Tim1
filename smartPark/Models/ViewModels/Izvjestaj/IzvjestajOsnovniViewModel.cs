using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class IzvjestajOsnovniViewModel
    {
        public int IzvjestajId { get; set; }
        public DateTime DatumGenerisanja { get; set; }
        public DateTime PeriodOd { get; set; }
        public DateTime PeriodDo { get; set; }
        public int UkupnoRezervacija { get; set; }
        public decimal UkupniPrihod { get; set; }
        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public TipIzvjestaja TipIzvjestaja { get; set; }

        public string TipTekst =>
            TipIzvjestaja == TipIzvjestaja.Korisnici ? "Izvještaj o korisnicima" : "Izvještaj o prihodima";
        public string PeriodTekst => $"{PeriodOd:dd.MM.yyyy} - {PeriodDo:dd.MM.yyyy}";
        public string DatumGenerisanjaTekst => DatumGenerisanja.ToString("dd.MM.yyyy HH:mm");
        public string UkupniPrihodTekst => $"{UkupniPrihod:F2} KM";
    }
}
