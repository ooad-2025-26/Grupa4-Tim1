using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Rezervacija
{
    public class RezervacijaOsnovniViewModel
    {
        public int RezervacijaId { get; set; }
        public string KorisnikId { get; set; } = string.Empty;
        public string KorisnikIme { get; set; } = string.Empty;
        public string KorisnikPrezime { get; set; } = string.Empty;
        public string KorisnikPunoIme => $"{KorisnikIme} {KorisnikPrezime}";
        public string ParkingAdresa { get; set; } = string.Empty;
        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public int? ParkingMjestoId { get; set; }
        public int? ParkingMjestoBroj { get; set; }
        public DateTime PocetakRezervacije { get; set; }
        public DateTime KrajRezervacije { get; set; }
        public decimal UkupnaCijena { get; set; }
        public StatusRezervacije StatusRezervacije { get; set; }
        public DateTime DatumKreiranjaRezervacije { get; set; }
        public TipParkinga ParkingTip { get; set; }

        public int BrojSati => (int)Math.Ceiling((KrajRezervacije - PocetakRezervacije).TotalHours);
        public bool JeAktivna => StatusRezervacije == StatusRezervacije.Aktivna;
        public bool MozeOtkazati => JeAktivna && PocetakRezervacije > DateTime.Now;
        public string StatusTekst =>
            StatusRezervacije switch
            {
                StatusRezervacije.Aktivna => "Aktivna",
                StatusRezervacije.Istekla => "Istekla",
                StatusRezervacije.Otkazana => "Otkazana",
                StatusRezervacije.Zavrsena => "Završena",
                _ => "Nepoznat",
            };
        public string StatusBoja =>
            StatusRezervacije switch
            {
                StatusRezervacije.Aktivna => "success",
                StatusRezervacije.Istekla => "warning",
                StatusRezervacije.Otkazana => "danger",
                StatusRezervacije.Zavrsena => "secondary",
                _ => "dark",
            };
    }
}
