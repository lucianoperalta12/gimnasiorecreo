using GymAdmin.Domain.Entities;
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
                }

                try
                {
                    await db.SaveChangesAsync(ct);
                }
                catch (Exception saveEx)
                {
                    await LogToDbAsync(
                        saveEx,
                        "BackgroundService: ExpireOverdueMembershipsAsync - SaveChangesAsync",
                        "BACKGROUND");
                    return;
                }

                foreach (var m in overdue)
                {
                    if (m.Alumno != null &&
                        await IsValidEmail(m.Alumno.Email))
                    {
                        try
                        {
                            await membershipService.SendExpirationEmailAsync(m);
                        }
                        catch (Exception ex)
                        {
                            var alumnoInfo = $"Email: {m.Alumno.Email} | Nombre: {m.Alumno.Nombre} {m.Alumno.Apellido} |" +
                                $" DNI: {m.Alumno.Dni} | Vencimiento: {m.FechaVencimiento:yyyy-MM-dd}";
                            await LogToDbAsync(
                                ex,
                                "BackgroundService: ExpireOverdueMembershipsAsync - SendExpirationEmailAsync",
                                alumnoInfo);
                        }
                    }
                }

                _logger.LogInformation(
                    "MembershipExpirationService: {Count} membresías marcadas como Vencida.",
                    overdue.Count);
            }
        }
        catch (Exception ex)
        {
            await LogToDbAsync(
                ex,
                "BackgroundService: ExpireOverdueMembershipsAsync",
                "BACKGROUND");
        }
    }

    private async Task LogToDbAsync(Exception exception, string path, string method)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.ErrorLogs.Add(new ErrorLog
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Path = path,
                Method = method,
                Timestamp = DateTime.Now
            });

            await db.SaveChangesAsync();

            // Mantener máximo 100 registros
            var oldestToKeep = await db.ErrorLogs
                .OrderByDescending(e => e.Id)
                .Skip(99)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (oldestToKeep > 0)
            {
                await db.ErrorLogs
                    .Where(e => e.Id < oldestToKeep)
                    .ExecuteDeleteAsync();
            }
        }
        catch { /* Si falla el log en DB no hay donde persistirlo */ }
    }

    private async Task<bool> IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch (Exception ex)
        {
            await LogToDbAsync(
                                ex,
                                "IsValidEmail",
                                email);
            return false;
        }
    }
}