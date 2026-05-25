using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.ParkingMjesto
{
    public class ParkingMjestoUrediViewModel
    {
        public int ParkingMjestoId { get; set; }

        [Required(ErrorMessage = "Broj mjesta je obavezan")]
        [Range(1, 1000, ErrorMessage = "Broj mjesta mora biti između 1 i 1000")]
        [Display(Name = "Broj parking mjesta")]
        public int BrojMjesta { get; set; }

        [Display(Name = "Status mjesta")]
        public StatusMjesta StatusMjesta { get; set; }

        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
    }
}
