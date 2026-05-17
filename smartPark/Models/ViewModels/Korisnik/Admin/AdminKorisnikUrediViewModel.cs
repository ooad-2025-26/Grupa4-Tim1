using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace smartPark.Models.ViewModels.Korisnik.Admin
{
    public class AdminKorisnikUrediViewModel
    {
        public string Id { get; set; } = null!;

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

        [Display(Name = "Aktivan")]
        public bool Aktivan { get; set; } = true;

        [Display(Name = "Zaključan")]
        public bool Zakljucan { get; set; }

        [Display(Name = "Uloga")]
        public string Uloga { get; set; } = null!;

        [Display(Name = "Broj vozačke")]
        public string? BrojVozacke { get; set; }

        [Display(Name = "Odgovorni parking")]
        public int? MenadzerOdgovorniParkingId { get; set; }

        [Display(Name = "Resetuj lozinku")]
        public bool ResetujLozinku { get; set; }

        [Display(Name = "Nova lozinka")]
        [DataType(DataType.Password)]
        public string? NovaLozinka { get; set; }

        // Dropdown liste
        public IEnumerable<SelectListItem>? DostupneUloge { get; set; }
        public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }
    }
}
