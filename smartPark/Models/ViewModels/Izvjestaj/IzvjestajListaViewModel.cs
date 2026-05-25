using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class IzvjestajListaViewModel
    {
        public List<IzvjestajOsnovniViewModel> Izvjestaji { get; set; } = new();
        public int UkupnoIzvjestaja { get; set; }
        public int? ParkingFilter { get; set; }
        public TipIzvjestaja? TipFilter { get; set; }

        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? DostupniParkinzi { get; set; }
    }
}
