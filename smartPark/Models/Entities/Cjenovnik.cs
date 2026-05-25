using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.Entities
{
    public class Cjenovnik
    {
        [Key]
        [Display(Name = "ID cjenovnika")]
        public int CjenovnikId { get; set; }

        [Required(ErrorMessage = "Naziv cjenovnika je obavezan")]
        [StringLength(100, ErrorMessage = "Naziv ne smije biti duži od 100 karaktera")]
        [Display(Name = "Naziv cjenovnika")]
        public string Naziv { get; set; } = string.Empty;

        [Display(Name = "ID parkinga")]
        public int? ParkingId { get; set; }

        [Display(Name = "Datum kreiranja")]
        [DataType(DataType.DateTime)]
        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        [Required]
        [Range(0.01, 1000, ErrorMessage = "Dnevna cijena mora biti izmedju 0.01 i 1000 KM")]
        [DataType(DataType.Currency)]
        [Display(Name = "Dnevna cijena (KM/h)")]
        public decimal CijenaDnevna { get; set; }

        [Required]
        [Range(0.01, 1000, ErrorMessage = "Noćna cijena mora biti izmedju 0.01 i 1000 KM")]
        [DataType(DataType.Currency)]
        [Display(Name = "Noćna cijena (KM/h)")]
        public decimal CijenaNocna { get; set; }

        [StringLength(50, ErrorMessage = "Zona ne smije biti duža od 50 karaktera")]
        [Display(Name = "Zona")]
        public string? Zona { get; set; }

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
        public virtual Parking? Parking { get; set; }
    }
}
