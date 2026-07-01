using GymAdmin.Application.Helpers;
using GymAdmin.Application.Services;

namespace GymAdmin.Api.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que orquesta la expiración periódica de membresías vencidas en todos
/// los gimnasios del sistema. Delega la lógica de negocio en <see cref="IMembershipService"/>
/// para mantener una única fuente de verdad compartida con los flujos on-demand.
/// </summary>
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

    /// <summary>
    /// Bucle principal del servicio. Invoca la expiración al arrancar la aplicación y luego la repite
    /// cada <c>6 horas</c> hasta que se cancele el token de parada.
    /// </summary>
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

    /// <summary>
    /// Dispara la expiración global de membresías vencidas delegando en
    /// <see cref="IMembershipService.ExpireOverdueMembershipsAsync"/> con <c>requester=null</c>
    /// y <c>gymId=null</c>, lo que aplica la operación sobre todos los gimnasios sin restricción.
    /// Cualquier excepción no controlada queda registrada en la tabla de <c>ErrorLog</c> mediante
    /// <see cref="ErrorLogHelper.LogAsync"/> para no interrumpir el ciclo del background service.
    /// </summary>
    /// <param name="ct">Token de cancelación propagado desde <see cref="ExecuteAsync"/>.</param>
    private async Task ExpireOverdueMembershipsAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Hora actual: {Now}", DateTime.UtcNow.AddHours(-3).ToString("yyyy-MM-dd HH:mm:ss"));

            using var scope = _scopeFactory.CreateScope();
            var membershipService = scope.ServiceProvider.GetRequiredService<IMembershipService>();

            await membershipService.ExpireOverdueMembershipsAsync();
        }
        catch (Exception ex)
        {
            await ErrorLogHelper.LogAsync(
                _scopeFactory,
                ex,
                "BackgroundService: ExpireOverdueMembershipsAsync",
                "BACKGROUND");
        }
    }
}