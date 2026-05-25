using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Parking.Admin
{
    public class AdminParkingUrediViewModel
    {
        public int ParkingId { get; set; }

        [Required(ErrorMessage = "Naziv parkinga je obavezan!")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Naziv parkinga")]
        public string Naziv { get; set; } = null!;

        [Required(ErrorMessage = "Adresa parkinga je obavezna!")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Adresa parkinga")]
        public string Adresa { get; set; } = null!;

        [Required]
        [Range(-90, 90)]
        [Display(Name = "Geografska širina")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        [Display(Name = "Geografska dužina")]
        public double Longitude { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Ukupan broj mjesta")]
        public int UkupnoMjesta { get; set; }

        [Required]
        [Range(0, 1000)]
        [Display(Name = "Slobodna mjesta")]
        public int SlobodnaMjesta { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Cijena po satu (KM)")]
        public decimal CijenaPoSatu { get; set; }

        [Required]
        [Display(Name = "Tip parkinga")]
        public TipParkinga TipParkinga { get; set; }

        [Display(Name = "Aktivan")]
        public bool Aktivan { get; set; }

        [StringLength(50)]
        [Display(Name = "Zona")]
        public string? Zona { get; set; }

        [StringLength(50)]
        [Display(Name = "Radno vrijeme")]
        public string? RadnoVrijeme { get; set; }

        [Display(Name = "Menadžer")]
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

        // Dropdown liste
        public IEnumerable<SelectListItem>? DostupniMenadzeri { get; set; }
    }
}
