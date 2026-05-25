using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class IzvjestajKreirajViewModel
    {
        [Required(ErrorMessage = "Parking je obavezan")]
        [Display(Name = "Parking")]
        public int ParkingId { get; set; }

        [Required(ErrorMessage = "Tip izvještaja je obavezan")]
        [Display(Name = "Tip izvještaja")]
        public TipIzvjestaja TipIzvjestaja { get; set; }

        [Required(ErrorMessage = "Period od je obavezan")]
        [DataType(DataType.Date)]
        [Display(Name = "Period od")]
        public DateTime PeriodOd { get; set; } = DateTime.Now.AddDays(-30);

        [Required(ErrorMessage = "Period do je obavezan")]
        [DataType(DataType.Date)]
        [Display(Name = "Period do")]
        public DateTime PeriodDo { get; set; } = DateTime.Now;

        [Display(Name = "Sačuvaj izvještaj u bazi")]
        public bool SacuvajUzBazi { get; set; } = true;

        [Display(Name = "Generiši PDF")]
        public bool GenerisiPdf { get; set; } = false;

        [Display(Name = "Generiši Excel")]
        public bool GenerisiExcel { get; set; } = false;

        // Dropdown liste
        public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }
    }
}
