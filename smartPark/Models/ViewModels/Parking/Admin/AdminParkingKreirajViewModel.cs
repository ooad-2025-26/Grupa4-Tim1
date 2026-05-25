using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Parking.Admin;

public class AdminParkingKreirajViewModel
{
    [Required(ErrorMessage = "Naziv je obavezan")]
    [StringLength(100, MinimumLength = 3)]
    public string Naziv { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresa je obavezna")]
    [StringLength(200, MinimumLength = 3)]
    public string Adresa { get; set; } = string.Empty;

    [Required]
    [Range(1, 10000)]
    public int UkupnoMjesta { get; set; }

    [DataType(DataType.Currency)]
    public decimal CijenaPoSatu { get; set; }

    public TipParkinga TipParkinga { get; set; }

    [StringLength(50)]
    public string? Zona { get; set; }

    [StringLength(50)]
    public string? RadnoVrijeme { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public bool Aktivan { get; set; } = true;

    public string? MenadzerId { get; set; }

    [Display(Name = "Defaultni cjenovnik")]
    public int? DefaultniCjenovnikId { get; set; }

    [Display(Name = "Dnevni cjenovnik")]
    public int? DnevniCjenovnikId { get; set; }

    [Display(Name = "Noćni cjenovnik")]
    public int? NocniCjenovnikId { get; set; }

    public IEnumerable<SelectListItem>? DostupniCjenovniciDefault { get; set; }
    public IEnumerable<SelectListItem>? DostupniCjenovniciDan { get; set; }
    public IEnumerable<SelectListItem>? DostupniCjenovniciNoc { get; set; }

    public IEnumerable<SelectListItem>? DostupniMenadzeri { get; set; }
}
