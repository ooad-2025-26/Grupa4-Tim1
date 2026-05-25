using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Notifikacija
{
    public class NotifikacijaPosaljiViewModel
    {
        [Required(ErrorMessage = "Primaoc je obavezan")]
        [EmailAddress(ErrorMessage = "Neispravan format emaila")]
        [Display(Name = "Email primaoca")]
        public string EmailPrimaoca { get; set; } = string.Empty;

        [Required(ErrorMessage = "Naslov je obavezan")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Naslov mora imati između 3 i 100 karaktera"
        )]
        [Display(Name = "Naslov emaila")]
        public string Naslov { get; set; } = string.Empty;

        [Required(ErrorMessage = "Poruka je obavezna")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "Poruka mora imati između 10 i 2000 karaktera"
        )]
        [Display(Name = "Sadržaj poruke")]
        public string Sadrzaj { get; set; } = string.Empty;

        [Display(Name = "HTML format")]
        public bool JeHtml { get; set; } = true;

        [Display(Name = "Pošalji kopiju meni")]
        public bool PosaljiKopijuMeni { get; set; } = false;
    }
}
