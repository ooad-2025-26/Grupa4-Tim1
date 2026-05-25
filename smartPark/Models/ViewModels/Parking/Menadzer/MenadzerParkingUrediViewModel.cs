using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels.Parking.Menadzer
{
    public class MenadzerParkingUrediViewModel
    {
        public int ParkingId { get; set; }
        public string Naziv { get; set; } = null!;
        public string Adresa { get; set; } = null!;

        [Required]
        [Range(0, 1000)]
        [Display(Name = "Broj slobodnih mjesta")]
        public int SlobodnaMjesta { get; set; }

        [Required]
        [Range(0.01, 1000)]
        [DataType(DataType.Currency)]
        [Display(Name = "Cijena po satu (KM)")]
        public decimal CijenaPoSatu { get; set; }

        [Display(Name = "Aktivan")]
        public bool Aktivan { get; set; }

        [StringLength(50)]
        [Display(Name = "Radno vrijeme")]
        public string? RadnoVrijeme { get; set; }
    }
}
