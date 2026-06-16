using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace GymAdmin.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body, string? from = null, string? bcc = null)
    {
        var host = _configuration["Email:Host"];
        var port = int.Parse(_configuration["Email:Port"] ?? "587");
        var user = _configuration["Email:User"];
        var pass = _configuration["Email:Password"];

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(!string.IsNullOrWhiteSpace(from) ? from : user!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        if (!string.IsNullOrWhiteSpace(bcc))
        {
            mailMessage.Bcc.Add(bcc);
        }

        await client.SendMailAsync(mailMessage);
    }
}
