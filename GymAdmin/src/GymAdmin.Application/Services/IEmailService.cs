using GymAdmin.Domain.Enums;

namespace GymAdmin.Application.Services;

public interface IEmailService
{
    //Task SendEmailAsync(string to, string subject, string body, string? from = null, string? bcc = null);
    Task SendEmailAsync(string to, string subject, string body, TipoCorreo tipo, string nombre, string apellido, string dni, int? gymId = null, string? from = null, string? bcc = null);
}