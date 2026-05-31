using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels;

public class PlacanjeViewModel : IValidatableObject
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
        @"^[A-Za-zŠĐČĆŽšđčćž\s\-]+$",
        ErrorMessage = "Unesite validno ime i prezime"
    )]
    [Display(Name = "Ime na kartici")]
    public string ImeNaKartici { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(DatumIsteka))
        {
            var parts = DatumIsteka.Split('/');
            if (parts.Length == 2 && int.TryParse(parts[0], out int month) && int.TryParse(parts[1], out int year))
            {
                if (year < 26 || (year == 26 && month < 7))
                {
                    yield return new ValidationResult("Datum isteka ne smije biti stariji od 07/26.", new[] { nameof(DatumIsteka) });
                }
            }
        }
    }
}
