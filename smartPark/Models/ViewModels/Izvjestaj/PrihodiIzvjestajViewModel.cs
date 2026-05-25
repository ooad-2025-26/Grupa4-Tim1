namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class PrihodiIzvjestajViewModel
    {
        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public DateTime PeriodOd { get; set; }
        public DateTime PeriodDo { get; set; }

        public int UkupnoRezervacija { get; set; }
        public decimal UkupniPrihod { get; set; }
        public decimal ProsjecniDnevniPrihod { get; set; }
        public decimal MaksimalniDnevniPrihod { get; set; }
        public decimal MinimalniDnevniPrihod { get; set; }
        public decimal ProsjecnaCijenaPoRezervaciji { get; set; }

        public List<PrihodiDnevna> DnevniPrihodi { get; set; } = new();

        public class PrihodiDnevna
        {
            public DateTime Datum { get; set; }
            public int BrojRezervacija { get; set; }
            public decimal Prihod { get; set; }
        }
    }
}
