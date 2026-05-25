using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.ParkingMjesto
{
    public class ParkingMjestoPromjenaStatusaViewModel
    {
        [Required]
        public int ParkingMjestoId { get; set; }

        [Required(ErrorMessage = "Novi status je obavezan")]
        [Display(Name = "Novi status")]
        public StatusMjesta NoviStatus { get; set; }

        [Display(Name = "Razlog promjene")]
        [StringLength(500)]
        public string? Razlog { get; set; }
    }
}
