using System.ComponentModel.DataAnnotations;

namespace smartPark.Models
{
    public class Notifikacija
    {
        [Key]
        [Display(Name = "ID notifikacije")]
        public int NotifikacijaId { get; set; }

        [Required(ErrorMessage = "Poruka je obavezna")]
        [StringLength(500, ErrorMessage = "Poruka ne smije biti duza od 500 karaktera")]
        [Display(Name = "Poruka")]
        public string Poruka { get; set; } = null!;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum slanja notifikacije")]
        public DateTime DatumSlanja { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "ID korisnika")]
        public string KorisnikId { get; set; } = null!;

        [Display(Name = "Korisnik")]
        public virtual Korisnik Korisnik { get; set; } = null!;
    }
}
