using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Korisnik;

public class PromijeniLozinkuViewModel
{
    [Required(ErrorMessage = "Trenutna lozinka je obavezna")]
    [DataType(DataType.Password)]
    [Display(Name = "Trenutna lozinka")]
    public string TrenutnaLozinka { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova lozinka je obavezna")]
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
    [Display(Name = "Nova lozinka")]
    public string NovaLozinka { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potvrda nove lozinke je obavezna")]
    [DataType(DataType.Password)]
    [Compare("NovaLozinka", ErrorMessage = "Lozinke se ne poklapaju")]
    [Display(Name = "Potvrdi novu lozinku")]
    public string PotvrdiLozinku { get; set; } = string.Empty;
}
