using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Korisnik;

public class ProfilViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ime je obavezno")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora imati između 2 i 50 karaktera")]
    [RegularExpression(
        @"^[A-ZŠĐČĆŽ][a-zšđčćž]{1,49}$",
        ErrorMessage = "Ime mora početi velikim slovom"
    )]
    [Display(Name = "Ime")]
    public string Ime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno")]
    [StringLength(
        50,
        MinimumLength = 2,
        ErrorMessage = "Prezime mora imati između 2 i 50 karaktera"
    )]
    [RegularExpression(
        @"^[A-ZŠĐČĆŽ][a-zšđčćž]{1,49}$",
        ErrorMessage = "Prezime mora početi velikim slovom"
    )]
    [Display(Name = "Prezime")]
    public string Prezime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email je obavezan")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu")]
    [StringLength(100, ErrorMessage = "Email ne može biti duži od 100 karaktera")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Uloga")]
    public string Uloga { get; set; } = string.Empty;

    [Display(Name = "Aktivan")]
    public bool Aktivan { get; set; }

    [Display(Name = "Datum registracije")]
    [DataType(DataType.DateTime)]
    public DateTime DatumRegistracije { get; set; }

    [StringLength(20, ErrorMessage = "Broj vozačke ne može biti duži od 20 karaktera")]
    [RegularExpression(@"^[a-zA-Z0-9\s-]{0,20}$", ErrorMessage = "Broj vozačke može sadržavati samo slova, brojeve, razmake i crtice")]
    [Display(Name = "Broj vozačke")]
    public string? BrojVozacke { get; set; }

    [Display(Name = "Odgovorni parking ID")]
    public int? MenadzerOdgovorniParkingId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Broj rezervacija ne može biti negativan")]
    public int BrojRezervacija { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Broj aktivnih rezervacija ne može biti negativan")]
    public int BrojAktivnihRezervacija { get; set; }
}
