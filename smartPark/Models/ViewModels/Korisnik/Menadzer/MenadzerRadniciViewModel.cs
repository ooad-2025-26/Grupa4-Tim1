using smartPark.Models.ViewModels.Korisnik.Shared;

namespace smartPark.Models.ViewModels.Korisnik.Menadzer
{
    public class MenadzerRadniciViewModel
    {
        public List<KorisnikListaStavkaViewModel> Radnici { get; set; } = new();
        public int UkupnoRadnika { get; set; }
        public int BrojAktivnihDanas { get; set; }
    }
}
