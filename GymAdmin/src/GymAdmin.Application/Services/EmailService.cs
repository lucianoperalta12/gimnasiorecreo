using System.Net;
using System.Net.Mail;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GymAdmin.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public EmailService(IConfiguration configuration, AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
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

    public async Task SendEmailAsync(string to, string subject, string body, TipoCorreo tipo, string nombre, string apellido, string dni, int? gymId = null, string? from = null, string? bcc = null)
    {
        var argentina = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentina);

        var log = new EmailLog
        {
            TipoCorreo = tipo,
            DestinatarioNombre = nombre,
            DestinatarioApellido = apellido,
            DestinatarioDni = dni,
            DestinatarioEmail = to,
            GymId = gymId,
            FechaEnvio = now
        };

        try
        {
            await SendEmailAsync(to, subject, body, from, bcc);
            log.Exitoso = true;
        }
        catch (Exception ex)
        {
            log.Exitoso = false;
            log.ErrorMensaje = ex.Message;
            throw;
        }
        finally
        {
            _context.EmailLogs.Add(log);
            await _context.SaveChangesAsync();

            // Mantener solo los últimos 1000 registros y eliminar el resto
            var totalCount = await _context.EmailLogs.CountAsync();
            if (totalCount > 1000)
            {
                var logsToDelete = await _context.EmailLogs
                    .OrderBy(x => x.FechaEnvio)
                    .ThenBy(x => x.Id)
                    .Take(totalCount - 1000)
                    .ToListAsync();

                _context.EmailLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}