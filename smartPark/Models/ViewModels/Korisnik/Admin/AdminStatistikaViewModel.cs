namespace smartPark.Models.ViewModels.Korisnik.Admin
{
    public class AdminStatistikaViewModel
    {
        public int UkupnoKorisnika { get; set; }
        public int UkupnoParkinga { get; set; }
        public int UkupnoRezervacija { get; set; }
        public decimal UkupniPrihod { get; set; }
        public int BrojVozaca { get; set; }
        public int BrojMenadzera { get; set; }
        public int BrojAdministratora { get; set; }
        public int BrojAktivnih { get; set; }
        public int BrojNeaktivnih { get; set; }
        public int BrojZakljucanih { get; set; }

        public Dictionary<DateTime, int> RegistracijePoDanima { get; set; } = new();

        public List<NedavnaAktivnost> NedavneAktivnosti { get; set; } = new();

        public class NedavnaAktivnost
        {
            public DateTime Datum { get; set; }
            public string Opis { get; set; } = null!;
            public string Tip { get; set; } = null!;
            public string? Korisnik { get; set; }
        }
    }
}
