using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models
{
    public class Izvjestaj
    {
        [Key]
        [Display(Name = "ID izvjestaja")]
        public int IzvjestajId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum generisanja")]
        public DateTime DatumGenerisanja { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Period od")]
        public DateTime PeriodOd { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Period do")]
        public DateTime PeriodDo { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Broj rezervacija ne moze biti negativan")]
        [Display(Name = "Ukupno rezervacija")]
        public int UkupnoRezervacija { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Prihod ne moze biti negativan")]
        [DataType(DataType.Currency)]
        [Display(Name = "Ukupni prihod (KM)")]
        public decimal UkupniPrihod { get; set; }

        [Required]
        [Display(Name = "ID parkinga")]
        public int ParkingId { get; set; }

        [Required]
        [Display(Name = "Tip izvjestaja")]
        public TipIzvjestaja TipIzvjestaja { get; set; }

        [Display(Name = "Parking")]
        public virtual Parking Parking { get; set; } = null!;
    }
}
