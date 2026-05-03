using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.Enums
{
    public enum Privilegije
    {
        [Display(Name = "Vozac")]
        Vozac = 1,

        [Display(Name = "Menadzer")]
        Menadzer = 2,

        [Display(Name = "Administrator")]
        Administrator = 3,
    }
}
