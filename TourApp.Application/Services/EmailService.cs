using System.Threading.Tasks;

namespace TourApp.Application.Services;

public class EmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Implementirati slanje emaila (SMTP, SendGrid, itd.)
        await Task.CompletedTask;
    }
} 