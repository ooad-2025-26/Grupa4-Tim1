using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Rezervacija;

public class RezervacijaOtkaziViewModel
{
    [Required]
    public int RezervacijaId { get; set; }

    [Required]
    public string KorisnikIme { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ParkingNaziv { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime PocetakRezervacije { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime KrajRezervacije { get; set; }

    [Range(0.01, 100000)]
    [DataType(DataType.Currency)]
    public decimal UkupnaCijena { get; set; }

    [StringLength(
        500,
        MinimumLength = 3,
        ErrorMessage = "Razlog mora imati između 3 i 500 karaktera"
    )]
    [Display(Name = "Razlog otkazivanja")]
    public string? RazlogOtkazivanja { get; set; }
}
