namespace smartPark.Models.ViewModels.Rezervacija
{
    public class RezervacijaDetaljiViewModel : RezervacijaOsnovniViewModel
    {
        public decimal ParkingCijenaPoSatu { get; set; }
        public string? QRKodBase64 { get; set; }
        public DateTime? QRKodDatumIsteka { get; set; }
        public bool QRKodIskoristen { get; set; }

        public decimal CijenaPoSatu => UkupnaCijena / BrojSati;
        public string VrijemePocetka => PocetakRezervacije.ToString("dd.MM.yyyy HH:mm");
        public string VrijemeKraja => KrajRezervacije.ToString("dd.MM.yyyy HH:mm");
        public string DatumKreiranja => DatumKreiranjaRezervacije.ToString("dd.MM.yyyy HH:mm");
    }
}
