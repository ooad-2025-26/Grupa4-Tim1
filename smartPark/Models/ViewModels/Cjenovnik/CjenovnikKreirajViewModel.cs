using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Cjenovnik;

public class CjenovnikKreirajViewModel
{
    [Required(ErrorMessage = "Naziv cjenovnika je obavezan")]
    [StringLength(100, ErrorMessage = "Naziv ne smije biti duži od 100 karaktera")]
    [Display(Name = "Naziv cjenovnika")]
    public string Naziv { get; set; } = string.Empty;

    [Display(Name = "Parking")]
    public int? ParkingId { get; set; }

    [Required(ErrorMessage = "Dnevna cijena je obavezna")]
    [Range(0.01, 1000, ErrorMessage = "Dnevna cijena mora biti između 0.01 i 1000 KM")]
    [DataType(DataType.Currency)]
    [Display(Name = "Dnevna cijena (KM/h)")]
    public decimal CijenaDnevna { get; set; }

    [Required(ErrorMessage = "Noćna cijena je obavezna")]
    [Range(0.01, 1000, ErrorMessage = "Noćna cijena mora biti između 0.01 i 1000 KM")]
    [DataType(DataType.Currency)]
    [Display(Name = "Noćna cijena (KM/h)")]
    public decimal CijenaNocna { get; set; }

    [StringLength(50, ErrorMessage = "Zona ne može biti duža od 50 karaktera")]
    public string? Zona { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DatumPocetka { get; set; } = DateTime.Now;

    [DataType(DataType.Date)]
    [DateGreaterThan("DatumPocetka", ErrorMessage = "Datum kraja mora biti poslije datuma početka")]
    public DateTime? DatumKraja { get; set; }

    public bool Aktivan { get; set; } = true;

    public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }
}
