using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Cjenovnik
{
    public class CjenovnikKreirajViewModel
    {
        [Required(ErrorMessage = "Parking je obavezan")]
        [Display(Name = "Parking")]
        public int ParkingId { get; set; }

        [Required(ErrorMessage = "Cijena po satu je obavezna")]
        [Range(0.01, 1000, ErrorMessage = "Cijena mora biti između 0.01 i 1000 KM")]
        [DataType(DataType.Currency)]
        [Display(Name = "Cijena po satu (KM)")]
        public decimal CijenaPoSatu { get; set; }

        [StringLength(50)]
        [Display(Name = "Zona")]
        public string? Zona { get; set; }

        [Required]
        [Display(Name = "Tip perioda")]
        public TipPerioda TipPerioda { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Datum početka važenja")]
        public DateTime DatumPocetka { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Datum kraja važenja (opciono)")]
        public DateTime? DatumKraja { get; set; }

        // Za dropdown listu parkinga
        public IEnumerable<SelectListItem>? ParkingLista { get; set; }
    }
}
