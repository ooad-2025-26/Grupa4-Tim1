namespace smartPark.Models.ViewModels.Korisnik.Shared
{
    public class KorisnikOsnovniViewModel
    {
        public string Id { get; set; } = null!;
        public string Ime { get; set; } = null!;
        public string Prezime { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PunoIme => $"{Ime} {Prezime}";
    }
}
