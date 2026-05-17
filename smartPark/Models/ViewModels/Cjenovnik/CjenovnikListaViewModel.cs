namespace smartPark.Models.ViewModels.Cjenovnik
{
    public class CjenovnikListaViewModel
    {
        public List<smartPark.Models.Entities.Cjenovnik> Cjenovnici { get; set; } = new();
        public int UkupnoCjenovnika { get; set; }
        public int AktivnihCjenovnika { get; set; }
        public int? ParkingFilter { get; set; }
    }
}
