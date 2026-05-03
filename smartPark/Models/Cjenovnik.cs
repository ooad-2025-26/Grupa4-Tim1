using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models
{
    public class Cjenovnik
    {
        [Key]
        [Display(Name = "ID cjenovnika")]
        public int CjenovnikId { get; set; }

        [Required]
        [Display(Name = "ID parkinga")]
        public int ParkingId { get; set; }

        [Required]
        [Range(0.01, 1000, ErrorMessage = "Cijena po satu mora biti izmedju 0.01 i 1000 KM")]
        [DataType(DataType.Currency)]
        [Display(Name = "Cijena po satu (KM)")]
        public decimal CijenaPoSatu { get; set; }

        [StringLength(50, ErrorMessage = "Zona ne smije biti duža od 50 karaktera")]
        [Display(Name = "Zona")]
        public string? Zona { get; set; }

        [Required]
        [Display(Name = "Tip perioda")]
        public TipPerioda TipPerioda { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum početka vazenja")]
        public DateTime DatumPocetka { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Datum kraja vazenja")]
        public DateTime? DatumKraja { get; set; } // Nullable – može biti trajno važeći

        [Required]
        [Display(Name = "Aktivan")]
        public bool Aktivan { get; set; } = true;

        [Display(Name = "Parking")]
        public virtual Parking Parking { get; set; } = null!;
    }
}
