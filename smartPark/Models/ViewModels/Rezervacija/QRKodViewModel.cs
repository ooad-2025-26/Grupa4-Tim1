using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Rezervacija;

public class QRKodViewModel
{
    public int QRKodId { get; set; }

    [Required]
    [StringLength(255)]
    public string Kod { get; set; } = string.Empty;

    [Required]
    public string Base64Slika { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DatumGenerisanja { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DatumIsteka { get; set; }

    public bool Iskoristen { get; set; }

    [Required]
    public int RezervacijaId { get; set; }

    [StringLength(100)]
    public string ParkingNaziv { get; set; } = string.Empty;

    [StringLength(10)]
    public string? ParkingMjestoBroj { get; set; }

    public bool JeVazeci => !Iskoristen && DatumIsteka > DateTime.Now;
    public string Status => JeVazeci ? "Važeći" : (Iskoristen ? "Iskorišten" : "Istekao");
}
