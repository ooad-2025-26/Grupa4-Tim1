using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models
{
    public class Parking
    {
        [Key]
        [Display(Name = "ID Parkinga")]
        public int ParkingId { get; set; }

        [Required(ErrorMessage = "Naziv parkinga je obavezan!")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Naziv mora imati izmedju 3 i 100 karaktera"
        )]
        [Display(Name = "Naziv parkinga")]
        public string Naziv { get; set; } = null!;

        [Required(ErrorMessage = "Adresa parkinga je obavezna!")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Adresa mora imati izmedju 3 i 100 karaktera"
        )]
        [Display(Name = "Adresa parkinga")]
        public string Adresa { get; set; } = null!;

        [Required]
        [Range(-90, 90, ErrorMessage = "Geografska sirina mora biti izmedju -90 i 90")]
        [Display(Name = "Geografska sirina")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Geografska duzina mora biti izmedju -180 i 180")]
        [Display(Name = "Geografska duzina")]
        public double Longitude { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Ukupan broj mjesta mora biti izmedju 1 i 1000")]
        [Display(Name = "Kapacitet parking prostora")]
        public int UkupnoMjesta { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Broj slobodnih mjesta mora biti izmedju 0 i 1000")]
        [Display(Name = "Slobodna mjesta")]
        public int SlobodnaMjesta { get; set; }

        [Required]
        [Range(0.01, 1000, ErrorMessage = "Cijena po satu mora biti izmedju 0.01 i 1000")]
        [DataType(DataType.Currency)]
        [Display(Name = "Cijena po satu (KM")]
        public decimal CijenaPoSatu { get; set; }

        [Required]
        [Display(Name = "Tip parkinga")]
        public TipParkinga TipParkinga { get; set; } = TipParkinga.Otvoreni;

        [Required]
        [Display(Name = "Aktivan")]
        public bool Aktivan { get; set; } = true;

        [Display(Name = "Datum kreiranja")]
        [DataType(DataType.DateTime)]
        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        [Display(Name = "ID menadzera")]
        public string? MenadzerID { get; set; }

        // NAVIGACIJA

        [Display(Name = "Menadzer")]
        public virtual Korisnik? Menadzer { get; set; }

        [Display(Name = "Parking mjesta")]
        public virtual ICollection<ParkingMjesto> ParkingMjesta { get; set; }

        [Display(Name = "Rezervacije")]
        public virtual ICollection<Rezervacija> Rezervacije { get; set; }

        public Parking()
        {
            ParkingMjesta = new HashSet<ParkingMjesto>();
            Rezervacije = new HashSet<Rezervacija>();
        }
    }
}
