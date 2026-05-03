using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.Enums
{
    public enum TipParkinga
    {
        [Display(Name = "Otvoreni parking")]
        Otvoreni = 1,

        [Display(Name = "Zatvoreni parking (garaza)")]
        Zatvoreni = 2,
    }
}
