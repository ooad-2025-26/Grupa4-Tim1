using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace smartPark.Models.ViewModels.Korisnik.Admin
{
    public class AdminKorisnikKreirajViewModel
    {
        [Required(ErrorMessage = "Ime je obavezno!")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Ime")]
        public string Ime { get; set; } = null!;

        [Required(ErrorMessage = "Prezime je obavezno!")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Prezime")]
        public string Prezime { get; set; } = null!;

        [Required(ErrorMessage = "Email je obavezan!")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Lozinka je obavezna!")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "Lozinka")]
        public string Lozinka { get; set; } = null!;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna!")]
        [DataType(DataType.Password)]
        [Compare("Lozinka", ErrorMessage = "Lozinke se ne poklapaju!")]
        [Display(Name = "Potvrdi lozinku")]
        public string PotvrdaLozinke { get; set; } = null!;

        [Required(ErrorMessage = "Uloga je obavezna!")]
        [Display(Name = "Uloga")]
        public string Uloga { get; set; } = "Vozac";

        [Display(Name = "Broj vozačke (samo za vozača)")]
        public string? BrojVozacke { get; set; }

        [Display(Name = "Parking (samo za menadžera)")]
        public int? MenadzerOdgovorniParkingId { get; set; }

        [Display(Name = "Pošalji email obavještenje")]
        public bool PosaljiEmailObavjestenje { get; set; } = true;

        // Dropdown liste
        public IEnumerable<SelectListItem>? DostupneUloge { get; set; }
        public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }
    }
}
