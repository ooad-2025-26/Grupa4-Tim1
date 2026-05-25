using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace smartPark.Models.ViewModels.Korisnik.Admin;

public class AdminKorisnikUrediViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ime je obavezno")]
    [StringLength(50, MinimumLength = 2)]
    public string Ime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno")]
    [StringLength(50, MinimumLength = 2)]
    public string Prezime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email je obavezan")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool Aktivan { get; set; }
    public bool Zakljucan { get; set; }

    [Required]
    public string Uloga { get; set; } = string.Empty;

    [StringLength(20)]
    [RegularExpression(@"^[a-zA-Z0-9\s-]{0,20}$", ErrorMessage = "Broj vozačke može sadržavati samo slova, brojeve, razmake i crtice")]
    public string? BrojVozacke { get; set; }

    [Range(1, int.MaxValue)]
    public int? MenadzerOdgovorniParkingId { get; set; }

    public List<int> MenadzerOdgovorniParkingIds { get; set; } = new();

    public IEnumerable<SelectListItem>? DostupneUloge { get; set; }
    public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }
}
