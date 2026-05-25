namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class IzvjestajDetaljiViewModel : IzvjestajOsnovniViewModel
    {
        // Dodatne informacije za parking
        public string ParkingAdresa { get; set; } = string.Empty;
        public int ParkingUkupnoMjesta { get; set; }
        public decimal ParkingCijenaPoSatu { get; set; }

        // Statistike po danima (za grafikone)
        public List<DnevnaStatistika> DnevneStatistike { get; set; } = new();

        // Statistike po satima (za grafikone)
        public List<SatnaStatistika> SatneStatistike { get; set; } = new();

        // Statistike po danima u sedmici
        public List<SedmicnaStatistika> SedmicneStatistike { get; set; } = new();

        // Prosječne vrijednosti
        public decimal ProsjecnaCijenaPoRezervaciji =>
            UkupnoRezervacija > 0 ? UkupniPrihod / UkupnoRezervacija : 0;
        public int BrojDana => (PeriodDo - PeriodOd).Days + 1;
        public decimal ProsjecniDnevniPrihod => BrojDana > 0 ? UkupniPrihod / BrojDana : 0;
        public double ProsjecnaDnevnaPopunjenost =>
            DnevneStatistike.Any() ? DnevneStatistike.Average(d => d.PopunjenostProcenat) : 0;
    }

    public class DnevnaStatistika
    {
        public DateTime Datum { get; set; }
        public int BrojRezervacija { get; set; }
        public decimal Prihod { get; set; }
        public double PopunjenostProcenat { get; set; }
        public string DatumTekst => Datum.ToString("dd.MM.yyyy");
    }

    public class SatnaStatistika
    {
        public int Sat { get; set; }
        public int BrojRezervacija { get; set; }
        public decimal Prihod { get; set; }
        public string SatTekst => $"{Sat}:00 - {Sat + 1}:00";
    }

    public class SedmicnaStatistika
    {
        public DayOfWeek Dan { get; set; }
        public int BrojRezervacija { get; set; }
        public decimal Prihod { get; set; }
        public string DanTekst =>
            Dan switch
            {
                DayOfWeek.Monday => "Ponedjeljak",
                DayOfWeek.Tuesday => "Utorak",
                DayOfWeek.Wednesday => "Srijeda",
                DayOfWeek.Thursday => "Četvrtak",
                DayOfWeek.Friday => "Petak",
                DayOfWeek.Saturday => "Subota",
                DayOfWeek.Sunday => "Nedjelja",
                _ => Dan.ToString(),
            };
    }
}
