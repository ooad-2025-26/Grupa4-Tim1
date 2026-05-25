using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class IzvjestajFilterViewModel
    {
        public int? ParkingId { get; set; }
        public TipIzvjestaja? TipIzvjestaja { get; set; }
        public DateTime? DatumOd { get; set; }
        public DateTime? DatumDo { get; set; }

        public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }
    }
}
