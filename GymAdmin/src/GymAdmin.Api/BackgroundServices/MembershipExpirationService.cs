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
            var membershipService = scope.ServiceProvider.GetRequiredService<IMembershipService>();

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
                            await membershipService.SendExpirationEmailAsync(m);
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