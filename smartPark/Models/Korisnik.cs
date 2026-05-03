using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace smartPark.Models
{
    public class Korisnik : IdentityUser
    {
        // OSNOVNI PODACI ZA SVA TRI AKTERA SISTEMA
        [Required(ErrorMessage = "Ime je obavezno!")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "Ime mora imati izmedu 2 i 50 karaktera"
        )]
        [Display(Name = "Ime")]
        public string Ime { get; set; } = null!;

        [Required(ErrorMessage = "Prezime je obavezno!")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "Prezime mora imati izmedu 2 i 50 karaktera"
        )]
        [Display(Name = "Prezime")]
        public string Prezime { get; set; } = null!;

        [Required(ErrorMessage = "Email adresa je obavezna")]
        [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu")]
        [StringLength(100, ErrorMessage = "Email ne može biti duži od 100 karaktera")]
        [Display(Name = "Email")]
        public override string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Aktivan")]
        public bool Aktivan { get; set; } = true; // inicijalna vrijednost za account

        [Required]
        [Display(Name = "Datum registracije korisnika sistema")]
        public DateTime DatumRegistracije { get; set; } = DateTime.Now;

        // DODATNI ATRIBTUI ZA ZASEBNE AKTERE
        [Display(Name = "Broj vozacke")]
        public string? BrojVozacke { get; set; } // za vozace

        [Display(Name = "Odgovorni Parking ID")]
        public int? MenadzerOdgovorniParkingId { get; set; } // za menadzere

        // NAVIGACIJA (LAKSE ZA EF CORE)
        // virtual zbog toga jer imamo u klasi sva tri aktera
        [Display(Name = "Parking (samo za menadžera)")]
        public virtual Parking? Parking { get; set; }

        [Display(Name = "Moje rezervacije")]
        public virtual ICollection<Rezervacija> Rezervacije { get; set; }

        [Display(Name = "Moje notifikacije")]
        public virtual ICollection<Notifikacija> Notifikacije { get; set; }

        // konstrukor zbog inicijalizacije liste za rezervacije i notifikacije da ne budu null
        public Korisnik()
        {
            Rezervacije = new HashSet<Rezervacija>();
            Notifikacije = new HashSet<Notifikacija>();
        }
    }
}
