using GymAdmin.Domain.Entities;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymAdmin.Application.Helpers;

/// <summary>
/// Helper centralizado para registrar excepciones en la tabla <c>ErrorLog</c>.
/// Mantiene un máximo de 100 registros, eliminando los más antiguos al superarse ese límite.
/// Los fallos internos se suprimen silenciosamente para no enmascarar el error original.
/// </summary>
public static class ErrorLogHelper
{
    /// <summary>
    /// Persiste la excepción dada en la tabla <c>ErrorLog</c> usando un scope propio de DI.
    /// </summary>
    /// <param name="scopeFactory">Factoría de scopes de DI para resolver <c>AppDbContext</c>.</param>
    /// <param name="exception">Excepción capturada a registrar.</param>
    /// <param name="path">Ruta o identificador del componente donde ocurrió el error.</param>
    /// <param name="method">Método o contexto adicional para facilitar el diagnóstico.</param>
    public static async Task LogAsync(
        IServiceScopeFactory scopeFactory,
        Exception exception,
        string path,
        string method)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.ErrorLogs.Add(new ErrorLog
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Path = path,
                Method = method,
                Timestamp = DateTime.Now.AddHours(-3)
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
}
