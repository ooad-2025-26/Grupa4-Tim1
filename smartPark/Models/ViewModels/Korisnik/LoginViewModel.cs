using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Korisnik;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email adresa je obavezna")]
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
    [Display(Name = "Lozinka")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Zapamti me")]
    public bool RememberMe { get; set; }
}
