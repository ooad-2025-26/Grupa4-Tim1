using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.Enums
{
    public enum TipIzvjestaja
    {
        [Display(Name = "Popunjenost parking mjesta")]
        Popunjenost = 1,

        [Display(Name = "Prihodi (sredstva)")]
        Prihodi = 2,
    }
}
