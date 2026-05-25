namespace smartPark.Models.ViewModels.Notifikacija
{
    public class NotifikacijaTemplateViewModel
    {
        public string Naslov { get; set; } = string.Empty;
        public string Sadrzaj { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;

        // Preddefinisani template-ovi
        public static NotifikacijaTemplateViewModel RezervacijaPotvrda(
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            decimal cijena
        )
        {
            return new NotifikacijaTemplateViewModel
            {
                Tip = "RezervacijaPotvrda",
                Naslov = "Potvrda rezervacije parkinga",
                Sadrzaj =
                    $@"
                    <h2>Poštovani/a {ime} {prezime},</h2>
                    <p>Vaša rezervacija je uspješno kreirana.</p>
                    <p><strong>Detalji rezervacije:</strong></p>
                    <ul>
                        <li><strong>Parking:</strong> {parkingNaziv}</li>
                        <li><strong>Datum i vrijeme početka:</strong> {pocetak.ToString("dd.MM.yyyy HH:mm")}</li>
                        <li><strong>Datum i vrijeme kraja:</strong> {kraj.ToString("dd.MM.yyyy HH:mm")}</li>
                        <li><strong>Ukupna cijena:</strong> {cijena:F2} KM</li>
                    </ul>
                    <p>Hvala Vam što koristite naše usluge!</p>
                    <p>SmartPark tim</p>
                ",
            };
        }

        public static NotifikacijaTemplateViewModel RezervacijaOtkazana(
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            string razlog
        )
        {
            return new NotifikacijaTemplateViewModel
            {
                Tip = "RezervacijaOtkazana",
                Naslov = "Rezervacija otkazana",
                Sadrzaj =
                    $@"
                    <h2>Poštovani/a {ime} {prezime},</h2>
                    <p>Vaša rezervacija je otkazana.</p>
                    <p><strong>Detalji otkazane rezervacije:</strong></p>
                    <ul>
                        <li><strong>Parking:</strong> {parkingNaziv}</li>
                        <li><strong>Datum i vrijeme početka:</strong> {pocetak.ToString("dd.MM.yyyy HH:mm")}</li>
                        <li><strong>Datum i vrijeme kraja:</strong> {kraj.ToString("dd.MM.yyyy HH:mm")}</li>
                    </ul>
                    <p><strong>Razlog otkazivanja:</strong> {razlog}</p>
                    <p>Za sva dodatna pitanja, kontaktirajte nas.</p>
                    <p>SmartPark tim</p>
                ",
            };
        }

        public static NotifikacijaTemplateViewModel RezervacijaUskoroIstice(
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj
        )
        {
            var preostaloVrijeme = (pocetak - DateTime.Now).Hours;
            return new NotifikacijaTemplateViewModel
            {
                Tip = "RezervacijaUskoroIstice",
                Naslov = "Podsetnik: Vaša rezervacija uskoro počinje",
                Sadrzaj =
                    $@"
                    <h2>Poštovani/a {ime} {prezime},</h2>
                    <p>Podsjećamo vas da vaša rezervacija počinje za {preostaloVrijeme} sati.</p>
                    <p><strong>Detalji rezervacije:</strong></p>
                    <ul>
                        <li><strong>Parking:</strong> {parkingNaziv}</li>
                        <li><strong>Datum i vrijeme početka:</strong> {pocetak.ToString("dd.MM.yyyy HH:mm")}</li>
                        <li><strong>Datum i vrijeme kraja:</strong> {kraj.ToString("dd.MM.yyyy HH:mm")}</li>
                    </ul>
                    <p>Hvala Vam što koristite naše usluge!</p>
                    <p>SmartPark tim</p>
                ",
            };
        }

        public static NotifikacijaTemplateViewModel NoviKorisnikRegistrovan(
            string ime,
            string prezime,
            string email
        )
        {
            return new NotifikacijaTemplateViewModel
            {
                Tip = "NoviKorisnikRegistrovan",
                Naslov = "Novi korisnik se registrovao",
                Sadrzaj =
                    $@"
                    <h2>Administratore,</h2>
                    <p>Novi korisnik se registrovao na sistem.</p>
                    <p><strong>Podaci o korisniku:</strong></p>
                    <ul>
                        <li><strong>Ime i prezime:</strong> {ime} {prezime}</li>
                        <li><strong>Email:</strong> {email}</li>
                    </ul>
                    <p>SmartPark tim</p>
                ",
            };
        }

        public static NotifikacijaTemplateViewModel Dobrodoslica(string ime, string prezime)
        {
            return new NotifikacijaTemplateViewModel
            {
                Tip = "Dobrodoslica",
                Naslov = "Dobrodošli u SmartPark!",
                Sadrzaj =
                    $@"
                    <h2>Poštovani/a {ime} {prezime},</h2>
                    <p>Dobrodošli u SmartPark sistem!</p>
                    <p>Vaš nalog je uspješno aktiviran. Sada možete:</p>
                    <ul>
                        <li>Kreirati rezervacije parkinga</li>
                        <li>Pratiti svoje rezervacije</li>
                        <li>Dobijati obavještenja o statusu rezervacija</li>
                    </ul>
                    <p>Za sva pitanja, slobodno nas kontaktirajte.</p>
                    <p>SmartPark tim</p>
                ",
            };
        }
    }
}
