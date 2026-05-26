using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Rezervacija;

public class RezervacijaPlacanjeViewModel
{
    // Nije required — rezervacija se kreira NAKON plaćanja
    public int RezervacijaId { get; set; }

    [Required]
    [StringLength(100)]
    public string ParkingNaziv { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime PocetakRezervacije { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime KrajRezervacije { get; set; }

    [Range(1, 720)]
    public int BrojSati { get; set; }

    [Range(0.01, 1000)]
    public decimal CijenaPoSatu { get; set; }

    [Range(0.01, 100000)]
    [DataType(DataType.Currency)]
    public decimal UkupnaCijena { get; set; }

    [Range(0, 100)]
    public int Popust { get; set; }
}
