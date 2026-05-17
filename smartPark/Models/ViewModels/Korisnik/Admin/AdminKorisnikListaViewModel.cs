using smartPark.Models.ViewModels.Korisnik.Shared;

namespace smartPark.Models.ViewModels.Korisnik.Admin
{
    public class AdminKorisnikListaViewModel
    {
        public List<KorisnikListaStavkaViewModel> Korisnici { get; set; } = new();
        public int UkupnoKorisnika { get; set; }
        public string? FilterUloga { get; set; }
        public string? FilterStatus { get; set; }
        public IEnumerable<string> DostupneUloge { get; set; } = new List<string>();
        public IEnumerable<string> DostupniStatusi { get; set; } =
            new List<string> { "Svi", "Aktivni", "Neaktivni" };

        // Statistika za admin dashboard
        public int BrojAktivnih { get; set; }
        public int BrojNeaktivnih { get; set; }
        public int BrojZakljucanih { get; set; }
        public int BrojVozaca { get; set; }
        public int BrojMenadzera { get; set; }
        public int BrojAdministratora { get; set; }
    }
}
