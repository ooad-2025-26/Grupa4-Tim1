namespace smartPark.Models.ViewModels.Korisnik.Shared
{
    public class KorisnikListaStavkaViewModel : KorisnikOsnovniViewModel
    {
        public string Uloga { get; set; } = null!;
        public bool Aktivan { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public bool JeZakljucan { get; set; }
        public int BrojRezervacija { get; set; }

        // Za prikaz statusa
        public string Status => Aktivan ? "Aktivan" : "Neaktivan";
        public string StatusBoja => Aktivan ? "success" : "danger";
        public string StatusZakljucanosti => JeZakljucan ? "Zaključan" : "Otključan";
    }
}
