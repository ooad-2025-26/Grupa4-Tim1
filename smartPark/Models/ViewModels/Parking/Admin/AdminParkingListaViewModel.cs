using smartPark.Models.Enums;
using smartPark.Models.ViewModels.Parking.Shared;

namespace smartPark.Models.ViewModels.Parking.Admin
{
    public class AdminParkingListaViewModel
    {
        public List<ParkingListaStavkaViewModel> Parkinzi { get; set; } = new();
        public int UkupnoParkinga { get; set; }
        public int AktivnihParkinga { get; set; }
        public int NeaktivnihParkinga { get; set; }
        public string? FilterStatus { get; set; }
        public string? FilterTip { get; set; }

        // Statistika
        public int UkupnoMjesta { get; set; }
        public int UkupnoSlobodnihMjesta { get; set; }
        public decimal UkupniDnevniPrihod { get; set; }
        public decimal ProsjecnaCijena { get; set; }

        public IEnumerable<string> DostupniStatusi { get; set; } =
            new List<string> { "Svi", "Aktivni", "Neaktivni" };
        public IEnumerable<TipParkinga> DostupniTipovi { get; set; } = new List<TipParkinga>();
    }
}
