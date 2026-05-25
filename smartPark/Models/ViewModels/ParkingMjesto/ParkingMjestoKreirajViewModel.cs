using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.ParkingMjesto
{
    public class ParkingMjestoKreirajViewModel
    {
        [Required(ErrorMessage = "Parking je obavezan")]
        [Display(Name = "Parking")]
        public int ParkingId { get; set; }

        [Required(ErrorMessage = "Broj mjesta je obavezan")]
        [Range(1, 1000, ErrorMessage = "Broj mjesta mora biti između 1 i 1000")]
        [Display(Name = "Broj parking mjesta")]
        public int BrojMjesta { get; set; }

        [Display(Name = "Status mjesta")]
        public StatusMjesta StatusMjesta { get; set; } = StatusMjesta.Slobodno;

        // Dropdown liste
        public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }

        // Automatsko kreiranje više mjesta
        [Display(Name = "Kreiraj više mjesta")]
        public bool KreirajViseMjesta { get; set; } = false;

        [Display(Name = "Broj mjesta za kreiranje")]
        [Range(1, 100)]
        public int BrojZaKreiranje { get; set; } = 1;
    }
}
