using GymAdmin.Application.Services;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Api.BackgroundServices;

public class StartupNotificationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StartupNotificationService> _logger;

    public StartupNotificationService(IServiceScopeFactory scopeFactory, ILogger<StartupNotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StartupNotificationService is starting.");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var users = await dbContext.Users.ToListAsync(stoppingToken);

            var activeCount = users.Count(u => u.Activo);
            var inactiveCount = users.Count(u => !u.Activo);

            var body = $@"
                <h2>Reporte de Inicio de Aplicación - GymAdmin</h2>
                <p>La aplicación ha iniciado correctamente.</p>
                
                <h3>Resumen de Usuarios:</h3>
                <ul>
                    <li><b>Activos:</b> {activeCount}</li>
                    <li><b>Inactivos:</b> {inactiveCount}</li>
                </ul>

                <h3>Detalle de Usuarios:</h3>
                <table border='1' cellpadding='5' style='border-collapse: collapse;'>
                    <thead>
                        <tr style='background-color: #f2f2f2;'>
                            <th>Nombre</th>
                            <th>Email</th>
                            <th>Rol</th>
                            <th>Estado</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var user in users)
            {
                body += $@"
                    <tr>
                        <td>{user.Nombre}</td>
                        <td>{user.Email}</td>
                        <td>{user.Rol}</td>
                        <td>{(user.Activo ? "Activo" : "Inactivo")}</td>
                    </tr>";
            }

            body += @"
                    </tbody>
                </table>
                <p style='color: #666; font-size: 0.8em; margin-top: 20px;'>Este es un mensaje automático generado al iniciar la aplicación.</p>";

            // await emailService.SendEmailAsync("lucianoperalta12@gmail.com", "Inicio de Aplicación - Reporte de Usuarios", body);
            
            _logger.LogInformation("Startup notification email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending startup notification email.");
        }
    }
}
