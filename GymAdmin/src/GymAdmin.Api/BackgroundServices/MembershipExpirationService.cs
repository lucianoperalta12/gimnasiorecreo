using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

            var now = DateTime.Now;
            _logger.LogInformation("Hora actual: {Now}", now.ToString("yyyy-MM-dd HH:mm:ss"));

            var updated = await db.Memberships
                .Where(m => m.Estado == MembershipStatus.Activa && m.FechaVencimiento < now)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.Estado, MembershipStatus.Vencida), ct);

            if (updated > 0)
                _logger.LogInformation("MembershipExpirationService: {Count} membresías marcadas como Vencida. ", updated);
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
}