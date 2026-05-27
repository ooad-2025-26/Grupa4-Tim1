namespace smartPark.Services.Interfaces;
public interface IEmailService
{
    Task PosaljiEmailAsync(string primalacEmail, string primalacIme, string naslov, string htmlTijelo);

    Task PosaljiPotvrduRezervacijeAsync(string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv, DateTime pocetak, DateTime kraj, decimal cijena);

    Task PosaljiPodsjetnikPocetkaAsync(string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv, DateTime pocetak, string? adresaParkinga);

    Task PosaljiPodsjetnikIstekaAsync(string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv, DateTime kraj);

    Task PosaljiObavijestPrekidaRezervacijeAsync(string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv, string status);

    Task PosaljiObavijestPocetkaRezervacijeAsync(string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv, DateTime pocetak, DateTime kraj, string? adresaParkinga);
}
