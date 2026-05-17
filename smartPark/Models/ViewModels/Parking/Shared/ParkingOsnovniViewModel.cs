using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Parking.Shared
{
    public class ParkingOsnovniViewModel
    {
        public int ParkingId { get; set; }
        public string Naziv { get; set; } = null!;
        public string Adresa { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int UkupnoMjesta { get; set; }
        public int SlobodnaMjesta { get; set; }
        public decimal CijenaPoSatu { get; set; }
        public TipParkinga TipParkinga { get; set; }
        public bool Aktivan { get; set; }

        public int ZauzetostProcenat =>
            UkupnoMjesta > 0 ? ((UkupnoMjesta - SlobodnaMjesta) * 100) / UkupnoMjesta : 0;

        public string Status => Aktivan ? "Aktivan" : "Neaktivan";
        public string StatusBoja => Aktivan ? "success" : "danger";
        public string ZauzetostBoja =>
            ZauzetostProcenat switch
            {
                >= 80 => "danger",
                >= 50 => "warning",
                _ => "success",
            };
    }
}
