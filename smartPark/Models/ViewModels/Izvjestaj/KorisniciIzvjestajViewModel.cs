using System;
using System.Collections.Generic;

namespace smartPark.Models.ViewModels.Izvjestaj
{
    public class KorisniciIzvjestajViewModel
    {
        public int ParkingId { get; set; }
        public string ParkingNaziv { get; set; } = string.Empty;
        public DateTime PeriodOd { get; set; }
        public DateTime PeriodDo { get; set; }

        public int UkupnoKorisnika { get; set; }
        public int NoviKorisnici { get; set; }
        public int UkupnoRezervacija { get; set; }
        
        public List<KorisniciDnevna> DnevnaStatistika { get; set; } = new();

        public class KorisniciDnevna
        {
            public DateTime Datum { get; set; }
            public int BrojAktivnihKorisnika { get; set; }
            public int BrojNoveRegistracije { get; set; }
        }
    }
}
