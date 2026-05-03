using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.Enums
{
    public enum StatusMjesta
    {
        [Display(Name = "Slobodno")]
        Slobodno = 1,

        [Display(Name = "Zauzeto")]
        Zauzeto = 2,
    }
}
