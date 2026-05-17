using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Cjenovnik
{
    public class CjenovnikUrediViewModel
    {
        [Required]
        public int CjenovnikId { get; set; }

        [Required]
        [Range(0.01, 1000)]
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
        public DateTime DatumPocetka { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Datum kraja važenja")]
        public DateTime? DatumKraja { get; set; }
    }
}
