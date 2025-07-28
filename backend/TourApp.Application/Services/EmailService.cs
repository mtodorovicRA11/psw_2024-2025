using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace TourApp.Application.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:Username"] ?? "";
        _smtpPassword = _configuration["Email:Password"] ?? "";
        _fromEmail = _configuration["Email:FromEmail"] ?? _smtpUsername;
        _fromName = _configuration["Email:FromName"] ?? "TourApp";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw to avoid breaking the application
            Console.WriteLine($"Error sending email to {to}: {ex.Message}");
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string username, string firstName)
    {
        var subject = "Dobrodošli u TourApp!";
        var body = GenerateWelcomeEmailBody(username, firstName);
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendBlockNotificationAsync(string to, string username)
    {
        var subject = "Obaveštenje o blokadi naloga - TourApp";
        var body = GenerateBlockNotificationBody(username);
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendUnblockNotificationAsync(string to, string username)
    {
        var subject = "Obaveštenje o odblokiranju naloga - TourApp";
        var body = GenerateUnblockNotificationBody(username);
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendTourPurchaseConfirmationAsync(string to, string username, string tourName, decimal price)
    {
        var subject = "Potvrda kupovine ture - TourApp";
        var body = GeneratePurchaseConfirmationBody(username, tourName, price);
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendTourCancellationNotificationAsync(string to, string username, string tourName)
    {
        var subject = "Obaveštenje o otkazivanju ture - TourApp";
        var body = GenerateCancellationNotificationBody(username, tourName);
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendProblemReportNotificationAsync(string to, string username, string tourName)
    {
        var subject = "Prijavljen problem sa turom - TourApp";
        var body = GenerateProblemReportBody(username, tourName);
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendTestEmailAsync(string to)
    {
        var subject = "Test Email - TourApp";
        var body = GenerateTestEmailBody();
        await SendEmailAsync(to, subject, body);
    }

    private string GenerateWelcomeEmailBody(string username, string firstName)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #1976d2;'>Dobrodošli u TourApp, {firstName}!</h2>
                    <p>Drago nam je što ste se pridružili našoj platformi za turističke ture.</p>
                    <p>Vaš korisnički nalog <strong>{username}</strong> je uspešno kreiran.</p>
                    <p>Možete početi sa:</p>
                    <ul>
                        <li>Pregledom dostupnih tura</li>
                        <li>Rezervacijom tura</li>
                        <li>Ocenjivanjem iskustava</li>
                    </ul>
                    <p>Hvala vam na poverenju!</p>
                    <p>Vaš TourApp tim</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateBlockNotificationBody(string username)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #f44336;'>Obaveštenje o blokadi naloga</h2>
                    <p>Poštovani korisniče,</p>
                    <p>Vaš nalog <strong>{username}</strong> je blokiran od strane administratora zbog neprikladnog ponašanja.</p>
                    <p>Za više informacija, kontaktirajte podršku.</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateUnblockNotificationBody(string username)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4caf50;'>Obaveštenje o odblokiranju naloga</h2>
                    <p>Poštovani korisniče,</p>
                    <p>Vaš nalog <strong>{username}</strong> je odblokiran i možete nastaviti sa korišćenjem platforme.</p>
                    <p>Hvala vam na razumevanju.</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }

    private string GeneratePurchaseConfirmationBody(string username, string tourName, decimal price)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4caf50;'>Potvrda kupovine ture</h2>
                    <p>Poštovani {username},</p>
                    <p>Uspešno ste kupili turu:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <p><strong>Naziv ture:</strong> {tourName}</p>
                        <p><strong>Cena:</strong> {price:C}</p>
                    </div>
                    <p>Uživajte u vašoj turi!</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateCancellationNotificationBody(string username, string tourName)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #ff9800;'>Obaveštenje o otkazivanju ture</h2>
                    <p>Poštovani {username},</p>
                    <p>Tura <strong>{tourName}</strong> je otkazana.</p>
                    <p>Izvinjavamo se na neprijatnosti.</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateProblemReportBody(string username, string tourName)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #f44336;'>Prijavljen problem sa turom</h2>
                    <p>Poštovani {username},</p>
                    <p>Primili smo prijavu problema sa turom <strong>{tourName}</strong>.</p>
                    <p>Naš tim će istražiti situaciju i kontaktirati vas u najkraćem mogućem roku.</p>
                    <p>Hvala vam na strpljenju.</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateTestEmailBody()
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #1976d2;'>Test Email - TourApp</h2>
                    <p>Ovo je test email za proveru email funkcionalnosti TourApp aplikacije.</p>
                    <p>Email konfiguracija je uspešno podesena!</p>
                    <p>Vreme slanja: {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
                    <p>TourApp tim</p>
                </div>
            </body>
            </html>";
    }
} 