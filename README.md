# 🅿 SmartPark — Sistem za upravljanje parking prostorima

SmartPark je moderna web aplikacija razvijena kao dio projekta iz predmeta **OOCD (Osnove objektno-orijentisanog dizajna)** na Elektrotehničkom fakultetu u Sarajevu. Sistem pruža napredno upravljanje parking prostorima, omogućava korisnicima (vozačima) brzu rezervaciju parking mjesta uz navigaciju i QR kod validaciju, dok menadžerima i administratorima daje detaljne analitičke izvještaje i kontrolu nad resursima.

---

## 🚀 Tehnološki stek

*   **Backend:** `.NET 10` (C#) / ASP.NET Core MVC
*   **Baza podataka:** `Entity Framework Core` / SQL Server (LocalDB)
*   **Autentifikacija i Autorizacija:** ASP.NET Core Identity (sa prilagođenom lokalizacijom na bosanski jezik)
*   **Servisi e-pošte:** `MailKit` SMTP klijent za automatske notifikacije i podsjetnike
*   **Frontend:** HTML5, CSS3, JavaScript (Bootstrap 5, Vanilla CSS, Bootstrap Icons)
*   **Validacija:** Integrisani QR kod generator i skener za menadžere na ulazu/izlazu

---

## 🔑 Testni nalozi

Za potrebe testiranja i prezentacije sistema, predefinisani su sljedeći nalozi sa dodijeljenim ulogama:

| Uloga | Email (Korisničko ime) | Lozinka |
| :--- | :--- | :--- |
| **Administrator** | `admin@smartpark.com` | `Admin123!` |
| **Menadžer / Zaposlenik** | `mhodzic6@etf.unsa.ba` | `MirzaHodzic2004@` |
| **Vozač** | `hodzic04mirza@gmail.com` | `MirzaHodzic2004@` |

---

## 🛠️ Ključne funkcionalnosti

### 1. Korisnički modul (Vozač)
*   Pretraga slobodnih parking mjesta na mapi i listi.
*   Kreiranje rezervacije parking mjesta za određeni vremenski period.
*   Prikaz detalja rezervacije sa automatskim QR kodom.
*   **Notifikacije:**
    *   Potvrda uspješne rezervacije sa QR kodom i linkom do lokacije na Google Maps.
    *   Podsjetnik 30 minuta prije početka rezervacije.
    *   Podsjetnik 30 minuta prije isteka rezervacije.
    *   Zahvalnica za korištenje sistema poslana odmah po završetku/otkazivanju rezervacije.

### 2. Upravljački modul (Menadžer)
*   Pregled statistike za dodijeljene parkinge u realnom vremenu (popunjenost, ukupno mjesta, prihod).
*   Kreiranje periodičnih analitičkih izvještaja o popunjenosti i finansijskim prihodima.
*   Eksport izvještaja u **PDF** i **Excel** formatima.
*   Skener QR kodova na ulazu za brzu validaciju i evidenciju dolazaka/odlazaka.

### 3. Administrativni modul (Administrator)
*   Upravljanje korisničkim nalozima (kreiranje, uređivanje, brisanje, zaključavanje).
*   Administracija parkinga i cjenovnika.
*   Pregled globalne statistike sistema i sistemskih aktivnosti.

---

## 💻 Kako pokrenuti projekat lokalno

### Preduslovi:
1. Instaliran [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
2. Instaliran [SQL Server Express / LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb).

### Koraci za pokretanje:

1.  **Klonirajte repozitorij:**
    ```bash
    git clone https://github.com/ooad-2025-26/Grupa4-Tim1.git
    cd Grupa4-Tim1/smartPark
    ```

2.  **Konfiguracija baze podataka:**
    Ažurirajte konekcioni string u `appsettings.json` ili ga postavite preko .NET User Secrets:
    ```bash
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=smartParkDb;Trusted_Connection=True;MultipleActiveResultSets=true"
    ```

3.  **Primijenite migracije i kreirajte bazu:**
    ```bash
    dotnet ef database update
    ```

4.  **Pokrenite aplikaciju:**
    ```bash
    dotnet run
    ```
    Aplikacija će biti dostupna na adresi `http://localhost:5000` (ili portu ispisanom u konzoli).

---

## 👥 Članovi tima
*   **Grupa 4 - Tim 1**
*   Elektrotehnički fakultet Sarajevo
