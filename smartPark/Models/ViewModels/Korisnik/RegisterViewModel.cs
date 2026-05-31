using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Korisnik;

public class RegisterViewModel
{
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

    [Required(ErrorMessage = "Lozinka je obavezna")]
    [DataType(DataType.Password)]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Lozinka mora imati između 6 i 100 karaktera"
    )]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Lozinka mora sadržavati barem jedno veliko slovo, jedno malo slovo i jedan broj"
    )]
    [Display(Name = "Lozinka")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potvrda lozinke je obavezna")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Lozinke se ne poklapaju")]
    [Display(Name = "Potvrdi lozinku")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Broj tablica je obavezan")]
    [StringLength(20, ErrorMessage = "Broj tablica ne može biti duži od 20 karaktera")]
    [RegularExpression(@"^[a-zA-Z0-9-]+$", ErrorMessage = "Broj tablica može sadržavati samo slova, brojeve i crtice")]
    [Display(Name = "Broj tablica")]
    public string BrojVozacke { get; set; } = string.Empty;
}
