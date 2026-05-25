using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Parking;

public class ParkingFormViewModel
{
    public int ParkingId { get; set; }

    [Required(ErrorMessage = "Naziv je obavezan")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Naziv mora imati između 3 i 100 karaktera"
    )]
    [Display(Name = "Naziv parkinga")]
    public string Naziv { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresa je obavezna")]
    [StringLength(
        200,
        MinimumLength = 3,
        ErrorMessage = "Adresa mora imati između 3 i 200 karaktera"
    )]
    [Display(Name = "Adresa")]
    public string Adresa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ukupan broj mjesta je obavezan")]
    [Range(1, 10000, ErrorMessage = "Ukupan broj mjesta mora biti između 1 i 10000")]
    [Display(Name = "Ukupno mjesta")]
    public int UkupnoMjesta { get; set; }

    [Required(ErrorMessage = "Broj slobodnih mjesta je obavezan")]
    [Range(0, 10000, ErrorMessage = "Broj slobodnih mjesta mora biti između 0 i 10000")]
    [Display(Name = "Slobodna mjesta")]
    public int SlobodnaMjesta { get; set; }

    [StringLength(50, ErrorMessage = "Zona ne može biti duža od 50 karaktera")]
    [Display(Name = "Zona")]
    public string? Zona { get; set; }

    [Required(ErrorMessage = "Tip parkinga je obavezan")]
    [Display(Name = "Tip parkinga")]
    public TipParkinga TipParkinga { get; set; }

    [StringLength(50, ErrorMessage = "Radno vrijeme ne može biti duže od 50 karaktera")]
    [Display(Name = "Radno vrijeme")]
    public string? RadnoVrijeme { get; set; }

    [Required(ErrorMessage = "Cijena po satu je obavezna")]
    [Range(0.01, 1000, ErrorMessage = "Cijena po satu mora biti između 0.01 i 1000 KM")]
    [DataType(DataType.Currency)]
    [Display(Name = "Cijena po satu (KM)")]
    public decimal CijenaPoSatu { get; set; }

    [Required(ErrorMessage = "Geografska širina je obavezna")]
    [Range(-90, 90, ErrorMessage = "Geografska širina mora biti između -90 i 90")]
    [Display(Name = "Geografska širina")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Geografska dužina je obavezna")]
    [Range(-180, 180, ErrorMessage = "Geografska dužina mora biti između -180 i 180")]
    [Display(Name = "Geografska dužina")]
    public double Longitude { get; set; }

    [Display(Name = "Aktivan")]
    public bool Aktivan { get; set; } = true;
}
