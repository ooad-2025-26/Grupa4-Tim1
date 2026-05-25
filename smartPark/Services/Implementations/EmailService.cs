using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ========== CORE SEND ==========

    public async Task PosaljiEmailAsync(string primalacEmail, string primalacIme, string naslov, string htmlTijelo)
    {
        var emailSection = _config.GetSection("Email");
        var host      = emailSection["SmtpHost"] ?? "smtp.gmail.com";
        var port      = int.Parse(emailSection["SmtpPort"] ?? "587");
        var useSsl    = bool.Parse(emailSection["UseSsl"] ?? "false");
        var username  = emailSection["Username"] ?? "";
        var password  = emailSection["Password"] ?? "";
        var fromName  = emailSection["FromName"] ?? "SmartPark";
        var fromAddr  = emailSection["FromAddress"] ?? username;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddr));
        message.To.Add(new MailboxAddress(primalacIme, primalacEmail));
        message.Subject = naslov;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlTijelo };
        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var secureOption = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(host, port, secureOption);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email uspješno poslan na {Email} - naslov: {Naslov}", primalacEmail, naslov);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri slanju emaila na {Email}", primalacEmail);
            throw;
        }
    }


    public async Task PosaljiPotvrduRezervacijeAsync(
        string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv,
        DateTime pocetak, DateTime kraj, decimal cijena)
    {
        var naslov = $"✅ SmartPark — Potvrda rezervacije #{rezervacijaId}";
        var mapsQuery = System.Uri.EscapeDataString(parkingNaziv);
        var mapsLink = $"https://www.google.com/maps/search/?api=1&query={mapsQuery}";
        var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=SmartPark_Rezervacija_ID_{rezervacijaId}";

        var html = $@"
<!DOCTYPE html>
<html lang='bs'>
<head><meta charset='UTF-8'><title>Potvrda rezervacije</title></head>
<body style='margin:0;padding:0;background:#f4f6f8;font-family:Inter,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' bgcolor='#f4f6f8'>
    <tr><td align='center' style='padding:40px 20px;'>
      <table width='600' cellpadding='0' cellspacing='0' bgcolor='#ffffff' style='border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);'>
        <!-- HEADER -->
        <tr><td bgcolor='#2563eb' style='padding:32px 40px;'>
          <h1 style='margin:0;color:#ffffff;font-size:22px;font-weight:700;'>🅿 SmartPark</h1>
          <p style='margin:8px 0 0;color:#bfdbfe;font-size:13px;'>Sistem za upravljanje parking prostorima</p>
        </td></tr>
        <!-- BODY -->
        <tr><td style='padding:40px;'>
          <h2 style='margin:0 0 8px;color:#1e293b;font-size:20px;'>Rezervacija potvrđena! ✅</h2>
          <p style='color:#64748b;margin:0 0 24px;font-size:14px;'>Poštovani/a <strong>{primalacIme}</strong>, Vaša rezervacija je uspješno kreirana.</p>

          <table width='100%' cellpadding='0' cellspacing='0' style='background:#f8fafc;border-radius:8px;padding:24px;margin-bottom:24px;'>
            <tr><td style='padding:8px 0;border-bottom:1px solid #e2e8f0;'>
              <span style='color:#64748b;font-size:12px;display:block;margin-bottom:2px;'>Broj rezervacije</span>
              <strong style='color:#1e293b;font-size:15px;'>#REZ-{rezervacijaId:D5}</strong>
            </td></tr>
            <tr><td style='padding:8px 0;border-bottom:1px solid #e2e8f0;'>
              <span style='color:#64748b;font-size:12px;display:block;margin-bottom:2px;'>Parking prostor</span>
              <strong style='color:#1e293b;font-size:15px;'>{parkingNaziv}</strong>
              <div style='margin-top:6px;'>
                <a href='{mapsLink}' target='_blank' style='display:inline-block;background:#e0f2fe;color:#0369a1;text-decoration:none;padding:5px 10px;border-radius:6px;font-size:11px;font-weight:600;'>
                  📍 Prikaži na Google Maps
                </a>
              </div>
            </td></tr>
            <tr><td style='padding:8px 0;border-bottom:1px solid #e2e8f0;'>
              <span style='color:#64748b;font-size:12px;display:block;margin-bottom:2px;'>Početak rezervacije</span>
              <strong style='color:#1e293b;font-size:15px;'>{pocetak:dd.MM.yyyy HH:mm}</strong>
            </td></tr>
            <tr><td style='padding:8px 0;border-bottom:1px solid #e2e8f0;'>
              <span style='color:#64748b;font-size:12px;display:block;margin-bottom:2px;'>Kraj rezervacije</span>
              <strong style='color:#1e293b;font-size:15px;'>{kraj:dd.MM.yyyy HH:mm}</strong>
            </td></tr>
            <tr><td style='padding:8px 0;'>
              <span style='color:#64748b;font-size:12px;display:block;margin-bottom:2px;'>Ukupna cijena</span>
              <strong style='color:#2563eb;font-size:18px;'>{cijena:F2} KM</strong>
            </td></tr>
          </table>

          <div style='text-align:center;margin:24px 0;background:#f8fafc;padding:20px;border-radius:8px;'>
            <img src='{qrCodeUrl}' alt='QR Kod Rezervacije' style='border:4px solid #ffffff;box-shadow:0 2px 8px rgba(0,0,0,0.1);border-radius:4px;width:150px;height:150px;' />
            <p style='margin:8px 0 0;font-size:12px;color:#64748b;'><strong>Skenirajte QR kod na ulazu za pristup parkingu</strong></p>
          </div>

          <p style='color:#64748b;font-size:13px;margin:0;'>Dobit ćete podsjetnik 30 minuta prije početka i 30 minuta prije isteka rezervacije.</p>
        </td></tr>
        <!-- FOOTER -->
        <tr><td bgcolor='#f8fafc' style='padding:24px 40px;border-top:1px solid #e2e8f0;'>
          <p style='margin:0;color:#94a3b8;font-size:12px;text-align:center;'>© {DateTime.Now.Year} SmartPark · Kontakt: <a href='mailto:help@smartpark.ba' style='color:#2563eb;'>help@smartpark.ba</a></p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        await PosaljiEmailAsync(primalacEmail, primalacIme, naslov, html);
    }


    public async Task PosaljiPodsjetnikPocetkaAsync(
        string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv,
        DateTime pocetak, string? adresaParkinga)
    {
        var naslov = $"⏰ SmartPark — Rezervacija počinje za 30 minuta";
        var mapsQuery = System.Uri.EscapeDataString($"{parkingNaziv} {adresaParkinga ?? ""}".Trim());
        var mapsLink = $"https://www.google.com/maps/search/?api=1&query={mapsQuery}";
        var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=SmartPark_Rezervacija_ID_{rezervacijaId}";

        var adresaHtml = !string.IsNullOrEmpty(adresaParkinga)
            ? $"<p style='color:#64748b;font-size:13px;margin:4px 0 0;'><strong>Adresa:</strong> {adresaParkinga}</p>"
            : "";

        var html = $@"
<!DOCTYPE html>
<html lang='bs'>
<head><meta charset='UTF-8'><title>Podsjetnik — početak rezervacije</title></head>
<body style='margin:0;padding:0;background:#f4f6f8;font-family:Inter,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' bgcolor='#f4f6f8'>
    <tr><td align='center' style='padding:40px 20px;'>
      <table width='600' cellpadding='0' cellspacing='0' bgcolor='#ffffff' style='border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);'>
        <tr><td bgcolor='#f59e0b' style='padding:32px 40px;'>
          <h1 style='margin:0;color:#ffffff;font-size:22px;font-weight:700;'>🅿 SmartPark</h1>
          <p style='margin:8px 0 0;color:#fef3c7;font-size:13px;'>Podsjetnik — rezervacija počinje uskoro</p>
        </td></tr>
        <tr><td style='padding:40px;'>
          <h2 style='margin:0 0 8px;color:#1e293b;font-size:20px;'>⏰ Vaša rezervacija počinje za 30 minuta!</h2>
          <p style='color:#64748b;margin:0 0 24px;font-size:14px;'>Poštovani/a <strong>{primalacIme}</strong>, podsjećamo Vas da Vaša parking rezervacija uskoro počinje.</p>
          
          <table width='100%' cellpadding='0' cellspacing='0' style='background:#fffbeb;border:1px solid #fde68a;border-radius:8px;padding:20px;margin-bottom:24px;'>
            <tr><td>
              <p style='margin:0 0 6px;color:#92400e;font-size:14px;'><strong>🅿 {parkingNaziv}</strong></p>
              {adresaHtml}
              <div style='margin-top:6px;margin-bottom:10px;'>
                <a href='{mapsLink}' target='_blank' style='display:inline-block;background:#fef3c7;color:#92400e;text-decoration:none;padding:5px 10px;border-radius:6px;font-size:11px;font-weight:600;border:1px solid #fde68a;'>
                  📍 Prikaži na Google Maps
                </a>
              </div>
              <p style='color:#92400e;font-size:14px;margin:10px 0 0;'><strong>Početak:</strong> {pocetak:dd.MM.yyyy HH:mm}</p>
              <p style='color:#64748b;font-size:12px;margin:4px 0 0;'>Rezervacija #{rezervacijaId:D5}</p>
            </td></tr>
          </table>

          <div style='text-align:center;margin:24px 0;background:#f8fafc;padding:20px;border-radius:8px;'>
            <img src='{qrCodeUrl}' alt='QR Kod Rezervacije' style='border:4px solid #ffffff;box-shadow:0 2px 8px rgba(0,0,0,0.1);border-radius:4px;width:150px;height:150px;' />
            <p style='margin:8px 0 0;font-size:12px;color:#64748b;'><strong>Skenirajte QR kod na ulazu za pristup parkingu</strong></p>
          </div>

          <p style='color:#64748b;font-size:13px;'>Molimo Vas da budete na lokaciji na vrijeme. Hvala što koristite SmartPark.</p>
        </td></tr>
        <tr><td bgcolor='#f8fafc' style='padding:24px 40px;border-top:1px solid #e2e8f0;'>
          <p style='margin:0;color:#94a3b8;font-size:12px;text-align:center;'>© {DateTime.Now.Year} SmartPark · <a href='mailto:help@smartpark.ba' style='color:#2563eb;'>help@smartpark.ba</a></p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        await PosaljiEmailAsync(primalacEmail, primalacIme, naslov, html);
    }


    public async Task PosaljiPodsjetnikIstekaAsync(
        string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv,
        DateTime kraj)
    {
        var naslov = $"⚠️ SmartPark — Rezervacija ističe za 30 minuta";
        var mapsQuery = System.Uri.EscapeDataString(parkingNaziv);
        var mapsLink = $"https://www.google.com/maps/search/?api=1&query={mapsQuery}";
        var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=SmartPark_Rezervacija_ID_{rezervacijaId}";

        var html = $@"
<!DOCTYPE html>
<html lang='bs'>
<head><meta charset='UTF-8'><title>Podsjetnik — istek rezervacije</title></head>
<body style='margin:0;padding:0;background:#f4f6f8;font-family:Inter,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' bgcolor='#f4f6f8'>
    <tr><td align='center' style='padding:40px 20px;'>
      <table width='600' cellpadding='0' cellspacing='0' bgcolor='#ffffff' style='border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);'>
        <tr><td bgcolor='#ef4444' style='padding:32px 40px;'>
          <h1 style='margin:0;color:#ffffff;font-size:22px;font-weight:700;'>🅿 SmartPark</h1>
          <p style='margin:8px 0 0;color:#fee2e2;font-size:13px;'>Podsjetnik — rezervacija uskoro ističe</p>
        </td></tr>
        <tr><td style='padding:40px;'>
          <h2 style='margin:0 0 8px;color:#1e293b;font-size:20px;'>⚠️ Vaša rezervacija ističe za 30 minuta!</h2>
          <p style='color:#64748b;margin:0 0 24px;font-size:14px;'>Poštovani/a <strong>{primalacIme}</strong>, molimo Vas da oslobodite parking mjesto na vrijeme.</p>
          <table width='100%' cellpadding='0' cellspacing='0' style='background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:20px;margin-bottom:24px;'>
            <tr><td>
              <p style='margin:0 0 6px;color:#991b1b;font-size:14px;'><strong>🅿 {parkingNaziv}</strong></p>
              <div style='margin-top:6px;margin-bottom:10px;'>
                <a href='{mapsLink}' target='_blank' style='display:inline-block;background:#fee2e2;color:#991b1b;text-decoration:none;padding:5px 10px;border-radius:6px;font-size:11px;font-weight:600;border:1px solid #fecaca;'>
                  📍 Prikaži na Google Maps
                </a>
              </div>
              <p style='color:#991b1b;font-size:14px;margin:10px 0 0;'><strong>Istek rezervacije:</strong> {kraj:dd.MM.yyyy HH:mm}</p>
              <p style='color:#64748b;font-size:12px;margin:4px 0 0;'>Rezervacija #{rezervacijaId:D5}</p>
            </td></tr>
          </table>

          <div style='text-align:center;margin:24px 0;background:#f8fafc;padding:20px;border-radius:8px;'>
            <img src='{qrCodeUrl}' alt='QR Kod Rezervacije' style='border:4px solid #ffffff;box-shadow:0 2px 8px rgba(0,0,0,0.1);border-radius:4px;width:150px;height:150px;' />
            <p style='margin:8px 0 0;font-size:12px;color:#64748b;'><strong>Skenirajte QR kod na ulazu za pristup parkingu</strong></p>
          </div>

          <p style='color:#64748b;font-size:13px;'>Prekoračenje vremena rezervacije može izazvati dodatne naknade. Hvala na razumijevanju.</p>
        </td></tr>
        <tr><td bgcolor='#f8fafc' style='padding:24px 40px;border-top:1px solid #e2e8f0;'>
          <p style='margin:0;color:#94a3b8;font-size:12px;text-align:center;'>© {DateTime.Now.Year} SmartPark · <a href='mailto:help@smartpark.ba' style='color:#2563eb;'>help@smartpark.ba</a></p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        await PosaljiEmailAsync(primalacEmail, primalacIme, naslov, html);
    }

    public async Task PosaljiObavijestPrekidaRezervacijeAsync(
        string primalacEmail, string primalacIme,
        int rezervacijaId, string parkingNaziv, string status)
    {
        var naslov = $"ℹ️ SmartPark — Obavještenje o završetku rezervacije #{rezervacijaId}";
        var statusTekst = status.ToLower() == "otkazana" ? "otkazana" : "istekla";
        var mapsQuery = System.Uri.EscapeDataString(parkingNaziv);
        var mapsLink = $"https://www.google.com/maps/search/?api=1&query={mapsQuery}";
        var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=SmartPark_Rezervacija_ID_{rezervacijaId}";

        var html = $@"
<!DOCTYPE html>
<html lang='bs'>
<head><meta charset='UTF-8'><title>Obavještenje o završetku rezervacije</title></head>
<body style='margin:0;padding:0;background:#f4f6f8;font-family:Inter,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' bgcolor='#f4f6f8'>
    <tr><td align='center' style='padding:40px 20px;'>
      <table width='600' cellpadding='0' cellspacing='0' bgcolor='#ffffff' style='border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);'>
        <!-- HEADER -->
        <tr><td bgcolor='#334155' style='padding:32px 40px;'>
          <h1 style='margin:0;color:#ffffff;font-size:22px;font-weight:700;'>🅿 SmartPark</h1>
          <p style='margin:8px 0 0;color:#cbd5e1;font-size:13px;'>Obavještenje o završetku rezervacije</p>
        </td></tr>
        <!-- BODY -->
        <tr><td style='padding:40px;'>
          <h2 style='margin:0 0 8px;color:#1e293b;font-size:20px;'>Rezervacija je završena ℹ️</h2>
          <p style='color:#64748b;margin:0 0 24px;font-size:14px;'>Poštovani/a <strong>{primalacIme}</strong>, Vaša rezervacija #{rezervacijaId:D5} za parking <strong>{parkingNaziv}</strong> je uspješno <strong>{statusTekst}</strong>.</p>

          <table width='100%' cellpadding='0' cellspacing='0' style='background:#f8fafc;border-radius:8px;padding:24px;margin-bottom:24px;border:1px solid #e2e8f0;'>
            <tr><td>
              <p style='margin:0;font-size:15px;color:#1e293b;line-height:1.6;'>
                Hvala Vam na korištenju naših usluga SmartPark sistema. Nadamo se da ste zadovoljni uslugom i da ćemo se ponovo družiti!
              </p>
              <div style='margin-top:16px;'>
                <a href='{mapsLink}' target='_blank' style='display:inline-block;background:#cbd5e1;color:#1e293b;text-decoration:none;padding:6px 12px;border-radius:6px;font-size:12px;font-weight:600;'>
                  📍 Lokacija parkinga Google Maps
                </a>
              </div>
            </td></tr>
          </table>

          <div style='text-align:center;margin:24px 0;background:#f8fafc;padding:20px;border-radius:8px;'>
            <img src='{qrCodeUrl}' alt='QR Kod Rezervacije' style='border:4px solid #ffffff;box-shadow:0 2px 8px rgba(0,0,0,0.1);border-radius:4px;width:150px;height:150px;' />
            <p style='margin:8px 0 0;font-size:12px;color:#64748b;'><strong>Arhivirani QR kod rezervacije</strong></p>
          </div>

          <p style='color:#64748b;font-size:13px;margin:0;'>Ukoliko imate bilo kakvih pitanja ili primjedbi, slobodno nas kontaktirajte.</p>
        </td></tr>
        <!-- FOOTER -->
        <tr><td bgcolor='#f8fafc' style='padding:24px 40px;border-top:1px solid #e2e8f0;'>
          <p style='margin:0;color:#94a3b8;font-size:12px;text-align:center;'>© {DateTime.Now.Year} SmartPark · Kontakt: <a href='mailto:help@smartpark.ba' style='color:#2563eb;'>help@smartpark.ba</a></p>
        </td></tr>
      </table>
    </td></tr>
  </table>
 </body>
</html>";

        await PosaljiEmailAsync(primalacEmail, primalacIme, naslov, html);
    }
}
