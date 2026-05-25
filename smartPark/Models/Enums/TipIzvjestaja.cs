using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.Enums
{
    public enum TipIzvjestaja
    {
        [Display(Name = "Prihodi")]
        Prihodi = 1,

        [Display(Name = "Korisnici")]
        Korisnici = 2,
    }
}
