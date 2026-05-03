using System.ComponentModel.DataAnnotations;
using smartPark.Models.Enums;

namespace smartPark.Models
{
    public class ParkingMjesto
    {
        [Key]
        [Display(Name = "ID parking mjesta")]
        public int ParkingMjestoId { get; set; }

        [Required]
        [Display(Name = "ID parking prostora")]
        public int ParkingId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Broj parking mjesta mora imati izmedju 1 i 1000")]
        [Display(Name = "Broj parking mjesta")]
        public int BrojMjesta { get; set; }

        [Required]
        [Display(Name = "Status mjesta")]
        public StatusMjesta StatusMjesta { get; set; } = StatusMjesta.Slobodno;

        [Required]
        [Display(Name = "Parking")]
        public virtual Parking Parking { get; set; } = null!;

        [Display(Name = "Trenutna rezervacija")]
        public virtual Rezervacija? TrenutnaRezervacija { get; set; }
    }
}
