using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using smartPark.Models.ViewModels.Notifikacija;
using smartPark.Services.Interfaces;

namespace smartPark.Services.Implementations
{
    public class NotifikacijaService : INotifikacijaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotifikacijaService> _logger;

        public NotifikacijaService(
            IConfiguration configuration,
            ILogger<NotifikacijaService> logger
        )
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> PosaljiEmailAsync(NotifikacijaPosaljiViewModel model)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"] ?? "localhost";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
                var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@smartpark.ba";
                var fromName = _configuration["Email:FromName"] ?? "SmartPark";

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(fromEmail, fromName);
                        message.To.Add(new MailAddress(model.EmailPrimaoca));
                        message.Subject = model.Naslov;
                        message.Body = model.Sadrzaj;
                        message.IsBodyHtml = model.JeHtml;

                        if (model.PosaljiKopijuMeni)
                        {
                            message.Bcc.Add(new MailAddress(fromEmail));
                        }

                        await client.SendMailAsync(message);
                    }
                }

                _logger.LogInformation($"Email uspješno poslan na {model.EmailPrimaoca}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri slanju emaila na {model.EmailPrimaoca}");
                return false;
            }
        }

        public async Task<bool> PosaljiTemplateEmailAsync(
            NotifikacijaTemplateViewModel template,
            string emailPrimaoca
        )
        {
            var model = new NotifikacijaPosaljiViewModel
            {
                EmailPrimaoca = emailPrimaoca,
                Naslov = template.Naslov,
                Sadrzaj = template.Sadrzaj,
                JeHtml = true,
                PosaljiKopijuMeni = false,
            };

            return await PosaljiEmailAsync(model);
        }

        public async Task<bool> PosaljiPotvrduRezervacijeAsync(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            decimal cijena
        )
        {
            var template = NotifikacijaTemplateViewModel.RezervacijaPotvrda(
                ime,
                prezime,
                parkingNaziv,
                pocetak,
                kraj,
                cijena
            );
            return await PosaljiTemplateEmailAsync(template, email);
        }

        public async Task<bool> PosaljiObavjestenjeOtkazanoAsync(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj,
            string razlog
        )
        {
            var template = NotifikacijaTemplateViewModel.RezervacijaOtkazana(
                ime,
                prezime,
                parkingNaziv,
                pocetak,
                kraj,
                razlog
            );
            return await PosaljiTemplateEmailAsync(template, email);
        }

        public async Task<bool> PosaljiPodsetnikRezervacijeAsync(
            string email,
            string ime,
            string prezime,
            string parkingNaziv,
            DateTime pocetak,
            DateTime kraj
        )
        {
            var template = NotifikacijaTemplateViewModel.RezervacijaUskoroIstice(
                ime,
                prezime,
                parkingNaziv,
                pocetak,
                kraj
            );
            return await PosaljiTemplateEmailAsync(template, email);
        }

        public async Task<bool> PosaljiDobrodoslicuAsync(string email, string ime, string prezime)
        {
            var template = NotifikacijaTemplateViewModel.Dobrodoslica(ime, prezime);
            return await PosaljiTemplateEmailAsync(template, email);
        }

        public async Task<bool> PosaljiAdminObavjestenjeNoviKorisnikAsync(
            string ime,
            string prezime,
            string email
        )
        {
            var adminEmail =
                _configuration["Email:AdminEmail"] ?? _configuration["Email:FromEmail"];
            if (string.IsNullOrEmpty(adminEmail))
                return false;

            var template = NotifikacijaTemplateViewModel.NoviKorisnikRegistrovan(
                ime,
                prezime,
                email
            );
            return await PosaljiTemplateEmailAsync(template, adminEmail);
        }

        public async Task<bool> TestirajEmailKonfiguracijuAsync(string testEmail)
        {
            var model = new NotifikacijaPosaljiViewModel
            {
                EmailPrimaoca = testEmail,
                Naslov = "SmartPark - Test email konfiguracije",
                Sadrzaj =
                    @"
                    <h2>Test email</h2>
                    <p>Ovo je testni email za provjeru konfiguracije email servera.</p>
                    <p>Ako ste primili ovaj email, konfiguracija je ispravna.</p>
                    <p>Lijep pozdrav,<br/>SmartPark tim</p>
                ",
                JeHtml = true,
                PosaljiKopijuMeni = false,
            };

            return await PosaljiEmailAsync(model);
        }
    }
}
