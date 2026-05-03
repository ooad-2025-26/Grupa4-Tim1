using System.ComponentModel.DataAnnotations;

namespace smartPark.Models
{
    public class QRKod
    {
        [Key]
        [Display(Name = "ID QR koda")]
        public int QRKodId { get; set; }

        [Required]
        [Display(Name = "QR kod (string podaci)")]
        public string Kod { get; set; } = null!;

        [Required]
        [Display(Name = "QR kod (base64 slika)")]
        public string Base64Slika { get; set; } = null!;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum generisanja")]
        public DateTime DatumGenerisanja { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum isteka")]
        public DateTime DatumIsteka { get; set; }

        [Display(Name = "Da li je kod iskoristen")]
        public bool Iskoristen { get; set; } = false;

        [Required]
        [Display(Name = "ID rezervacije")]
        public int RezervacijaId { get; set; }

        [Display(Name = "Rezervacija")]
        public virtual Rezervacija Rezervacija { get; set; } = null!;
    }
}
