using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace smartPark.Models.ViewModels.Rezervacija;

public class RezervacijaKreirajViewModel
{
    [Required(ErrorMessage = "Parking je obavezan")]
    [Range(1, int.MaxValue, ErrorMessage = "Parking mora biti odabran")]
    public int ParkingId { get; set; }

    [Required(ErrorMessage = "Datum početka je obavezan")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Datum i vrijeme početka")]
    public DateTime PocetakRezervacije { get; set; } = DateTime.Now.AddHours(1);

    [Required(ErrorMessage = "Datum kraja je obavezan")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Datum i vrijeme kraja")]
    [DateGreaterThan(
        "PocetakRezervacije",
        ErrorMessage = "Datum kraja mora biti poslije datuma početka"
    )]
    public DateTime KrajRezervacije { get; set; } = DateTime.Now.AddHours(2);

    [Range(0, 100, ErrorMessage = "Popust mora biti između 0 i 100%")]
    [Display(Name = "Popust (%)")]
    public int Popust { get; set; } = 0;

    [Range(0.01, 1000, ErrorMessage = "Cijena po satu mora biti između 0.01 i 1000 KM")]
    public decimal CijenaPoSatu { get; set; }

    public int BrojSati => (int)Math.Ceiling((KrajRezervacije - PocetakRezervacije).TotalHours);
    public decimal UkupnaCijena => CijenaPoSatu * BrojSati * (1 - Popust / 100m);

    public int? ParkingMjestoId { get; set; }
    public List<SelectListItem> DostupnaParkingMjesta { get; set; } = new();

    public IEnumerable<SelectListItem>? DostupniParkinzi { get; set; }
}
