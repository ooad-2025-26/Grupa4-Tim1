using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Cjenovnik;

public class CjenovnikManagerViewModel
{
    public int CjenovnikId { get; set; }

    [Required(ErrorMessage = "Naziv cjenovnika je obavezan")]
    [StringLength(100, ErrorMessage = "Naziv ne smije biti duži od 100 karaktera")]
    public string Naziv { get; set; } = string.Empty;

    public int? ParkingId { get; set; }

    [Required(ErrorMessage = "Cijena je obavezna")]
    [Range(0.01, 1000)]
    [DataType(DataType.Currency)]
    public decimal CijenaPoSatu { get; set; }

    [StringLength(50)]
    public string? Zona { get; set; }

    [Required]
    public TipPerioda TipPerioda { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DatumPocetka { get; set; } = DateTime.Now;

    [DataType(DataType.Date)]
    [DateGreaterThan("DatumPocetka")]
    public DateTime? DatumKraja { get; set; }

    public bool Aktivan { get; set; } = true;
}
