using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GymAdmin.Application.Services;

namespace GymAdmin.Api.BackgroundServices;

public class MembershipExpirationService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MembershipExpirationService> _logger;

    public MembershipExpirationService(IServiceScopeFactory scopeFactory, ILogger<MembershipExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MembershipExpirationService iniciado. Intervalo: {Interval}h.", Interval.TotalHours);

        // Ejecutar inmediatamente al iniciar la app y luego cada 6 horas
        while (!stoppingToken.IsCancellationRequested)
        {
            await ExpireOverdueMembershipsAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ExpireOverdueMembershipsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = DateTime.Now;
            _logger.LogInformation("Hora actual: {Now}", now.ToString("yyyy-MM-dd HH:mm:ss"));

            var overdue = await db.Memberships
                .Include(m => m.Alumno)
                .Include(m => m.Gym)
                .Where(m => m.Estado == MembershipStatus.Activa && m.FechaVencimiento < now)
                .ToListAsync(ct);

            if (overdue.Count > 0)
            {
                foreach (var m in overdue)
                {
                    m.Estado = MembershipStatus.Vencida;

                    if (m.Alumno != null && !string.IsNullOrWhiteSpace(m.Alumno.Email) && IsValidEmail(m.Alumno.Email))
                    {
                        try
                        {
                            var studentName = m.Alumno.Nombre;
                            var gymName = m.Gym?.Nombre ?? "el gimnasio";
                            var gymColor = m.Gym?.ColorPrincipalHex ?? "#ff6600";

                            var logoHtml = !string.IsNullOrWhiteSpace(m.Gym?.LogoUrl)
                                ? $"<div style='text-align: center; margin-bottom: 20px;'><img src='{m.Gym.LogoUrl}' alt='{gymName}' style='max-height: 80px; border-radius: 8px;' /></div>"
                                : "";
                            logoHtml = logoHtml + $"<div style='font-size: 24px; font-weight: bold; color: {gymColor}; text-align: center; margin-bottom: 20px;'>{gymName}</div>";

                            var body = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 40px 20px; color: #333333;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); overflow: hidden; border-top: 5px solid {gymColor};"">
        <div style=""padding: 30px; text-align: center;"">
            {logoHtml}
            <h2 style=""color: #2c3e50; margin-top: 10px; font-weight: 600;"">Aviso de Vencimiento</h2>
        </div>
        <div style=""padding: 0 40px 40px 40px; line-height: 1.6; font-size: 16px;"">
            <p>Hola <strong>{studentName}</strong>, ¿cómo estás?</p>
            <p>Te recordamos que tu cuota del gimnasio se encuentra vencida. Te pedimos que regularices el pago para mantener tu acceso activo y seguir disfrutando de las actividades del gimnasio.</p>
            <p style=""margin-top: 30px;"">Muchas gracias.</p>
        </div>
        <div style=""background-color: #2c3e50; color: #ffffff; padding: 20px; text-align: center; font-size: 12px;"">
            Este es un correo automático enviado por {gymName}. Por favor, no respondas a este mensaje.
        </div>
    </div>
</div>";

                            await emailService.SendEmailAsync(
                                to: m.Alumno.Email,
                                subject: "Vencimiento de cuota de gimnasio",
                                body: body,
                                from: "fitcenter.manager@gmail.com",
                                bcc: "lucianoperalta12@gmail.com"
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al enviar email de vencimiento para membresía {Id}", m.Id);
                        }
                    }
                }

                await db.SaveChangesAsync(ct);
                _logger.LogInformation("MembershipExpirationService: {Count} membresías marcadas como Vencida.", overdue.Count);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown normal, no loguear como error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MembershipExpirationService: error al expirar membresías.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}