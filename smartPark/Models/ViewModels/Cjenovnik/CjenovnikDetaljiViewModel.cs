using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.Cjenovnik
{
    public class CjenovnikDetaljiViewModel
    {
        public int CjenovnikId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public decimal CijenaPoSatu { get; set; }
        public string? Zona { get; set; }
        public TipPerioda TipPerioda { get; set; }
        public DateTime DatumPocetka { get; set; }
        public DateTime? DatumKraja { get; set; }
        public bool Aktivan { get; set; }
        public bool JeVazeci { get; set; }

        public string Status => JeVazeci ? "VAŽEĆI" : (Aktivan ? "NEVAŽEĆI" : "DEAKTIVIRAN");
        public string StatusBoja => JeVazeci ? "success" : (Aktivan ? "warning" : "danger");
    }
}
