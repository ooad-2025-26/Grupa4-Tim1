using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Rezervacija
{
    public class RezervacijaUrediViewModel
    {
        public int RezervacijaId { get; set; }

        [Required(ErrorMessage = "Datum početka je obavezan")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum i vrijeme početka")]
        public DateTime PocetakRezervacije { get; set; }

        [Required(ErrorMessage = "Datum kraja je obavezan")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum i vrijeme kraja")]
        public DateTime KrajRezervacije { get; set; }

        [Range(0, 1000)]
        [Display(Name = "Popust (%)")]
        public int Popust { get; set; }

        [Display(Name = "Status rezervacije")]
        public StatusRezervacije StatusRezervacije { get; set; }

        [Display(Name = "Parking mjesto")]
        public int? ParkingMjestoId { get; set; }

        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public decimal CijenaPoSatu { get; set; }

        public int BrojSati => (int)Math.Ceiling((KrajRezervacije - PocetakRezervacije).TotalHours);
        public decimal UkupnaCijena => CijenaPoSatu * BrojSati * (1 - Popust / 100m);

        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? DostupnaParkingMjesta { get; set; }
    }
}
