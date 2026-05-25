using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels;

public class PlacanjeViewModel
{
    [Required(ErrorMessage = "Broj kartice je obavezan")]
    [RegularExpression(@"^\d{4} \d{4} \d{4} \d{4}$", ErrorMessage = "Format: 1234 5678 9012 3456")]
    [Display(Name = "Broj kartice")]
    public string BrojKartice { get; set; } = string.Empty;

    [Required(ErrorMessage = "Datum isteka je obavezan")]
    [RegularExpression(@"^(0[1-9]|1[0-2])/[0-9]{2}$", ErrorMessage = "Format: MM/YY")]
    [Display(Name = "Datum isteka")]
    public string DatumIsteka { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV je obavezan")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV mora imati 3 cifre")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ime na kartici je obavezno")]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(
        @"^[A-ZŠĐČĆŽ][a-zšđčćž]+ [A-ZŠĐČĆŽ][a-zšđčćž]+$",
        ErrorMessage = "Unesite puno ime i prezime"
    )]
    [Display(Name = "Ime na kartici")]
    public string ImeNaKartici { get; set; } = string.Empty;
}
