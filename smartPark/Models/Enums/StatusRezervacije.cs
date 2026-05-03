using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.Enums
{
    public enum StatusRezervacije
    {
        [Display(Name = "Aktivna")]
        Aktivna = 1,

        [Display(Name = "Istekla")]
        Istekla = 2,

        [Display(Name = "Otkazana")]
        Otkazana = 3,

        [Display(Name = "Zavrsena")]
        Zavrsena = 4,
    }
}
