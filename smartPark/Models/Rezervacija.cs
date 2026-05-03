using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models
{
    public class Rezervacija
    {
        [Key]
        [Display(Name = "ID rezervacije")]
        public int RezervacijaId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Pocetak rezeravcije")]
        public DateTime PocetakRezervacije { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Kraj rezeravcije")]
        public DateTime KrajRezervacije { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Cijena mora biti izmedju 0 i 1000 KM")]
        [DataType(DataType.Currency)]
        [Display(Name = "Ukupna cijena (KM)")]
        public decimal UkupnaCijena { get; set; }

        [Required]
        [Display(Name = "Status rezervacije")]
        public StatusRezervacije StatusRezervacije { get; set; } = StatusRezervacije.Aktivna;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum kreiranja rezeravcije")]
        public DateTime DatumKreiranjaRezervacije { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "ID korisnika")]
        public string KorisnikId { get; set; } = null!;

        [Required]
        [Display(Name = "ID parkinga")]
        public int ParkingId { get; set; }

        [Display(Name = "ID parking mjesta")]
        public int? ParkingMjestoId { get; set; }

        [Display(Name = "Korisnik")]
        public virtual Korisnik Korisnik { get; set; } = null!;

        [Display(Name = "Parking")]
        public virtual Parking Parking { get; set; } = null!;

        [Display(Name = "Parking mjesto")]
        public virtual ParkingMjesto? ParkingMjesto { get; set; } = null!;

        [Display(Name = "QR kod rezervacije")]
        public virtual QRKod? QRKodRezervacije { get; set; }
    }
}
