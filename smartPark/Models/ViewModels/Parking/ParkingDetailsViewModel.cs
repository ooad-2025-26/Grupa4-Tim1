using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Parking;

public class ParkingDetailsViewModel
{
    public int ParkingId { get; set; }

    [Required]
    [StringLength(100)]
    public string Naziv { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Adresa { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int UkupnoMjesta { get; set; }

    [Range(0, 10000)]
    public int SlobodnaMjesta { get; set; }

    [Range(0, 1000)]
    public decimal CijenaPoSatu { get; set; }

    public TipParkinga TipParkinga { get; set; }

    [StringLength(50)]
    public string? Zona { get; set; }

    public bool Aktivan { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [StringLength(50)]
    public string? RadnoVrijeme { get; set; }

    public int ZauzetostProcenat =>
        UkupnoMjesta > 0 ? ((UkupnoMjesta - SlobodnaMjesta) * 100) / UkupnoMjesta : 0;
    public string TipParkingaTekst =>
        TipParkinga == TipParkinga.Otvoreni ? "Otvoreni" : "Zatvoreni (garaža)";
}
