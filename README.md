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
| **Menadžer / Zaposlenik** | `menadzer@smartpark.com` | `Menadzer123!` |
| **Vozač** | `vozac@gmail.com` | `Vozac123!` |

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

## 🌐 Aplikacija je dostupna online

SmartPark sistem je deployovan i dostupan na sljedećoj adresi:

**🔗 [smartpark.hodzicmirza.com](https://smartpark.hodzicmirza.com)**

---

## 👥 Članovi tima
*   **Grupa 4 - Tim 1**
*   Elektrotehnički fakultet Sarajevo