using smartPark.Models.Enums;

namespace smartPark.Models.ViewModels.ParkingMjesto
{
    public class ParkingMjestoOsnovniViewModel
    {
        public int ParkingMjestoId { get; set; }
        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public int BrojMjesta { get; set; }
        public StatusMjesta StatusMjesta { get; set; }

        public string StatusTekst =>
            StatusMjesta switch
            {
                StatusMjesta.Slobodno => "Slobodno",
                StatusMjesta.Zauzeto => "Zauzeto",
                _ => "Nepoznato",
            };

        public string StatusBoja =>
            StatusMjesta switch
            {
                StatusMjesta.Slobodno => "success",
                StatusMjesta.Zauzeto => "danger",
                _ => "dark",
            };

        public bool JeSlobodno => StatusMjesta == StatusMjesta.Slobodno;
        public bool JeZauzeto => StatusMjesta == StatusMjesta.Zauzeto;
    }
}
