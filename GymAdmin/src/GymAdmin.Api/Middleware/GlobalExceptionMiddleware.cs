using System.Data.Common;
using System.Net;
using System.Text.Json;
using GymAdmin.Domain.Entities;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Api.Middleware;

/// <summary>
/// Middleware global para la captura y manejo unificado de excepciones en la API.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Método de entrada que intercepta la petición HTTP y captura cualquier excepción no controlada.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Mapea la excepción a un código de estado HTTP adecuado y formatea la respuesta en formato JSON.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException => (HttpStatusCode.Conflict, exception.Message),
            DbException dbEx when IsAttendanceSchemaError(dbEx) =>
                (HttpStatusCode.Conflict, "La base de datos no tiene aplicada la actualizacion de asistencia. Debe crear la tabla Ingresos y la columna Memberships.IngresosUtilizados."),
            DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("FOREIGN KEY") == true || dbEx.Message.Contains("FOREIGN KEY") =>
                (HttpStatusCode.Conflict, "No se puede eliminar el registro porque tiene otros datos asociados (ej: membresias, rutinas o pagos)."),
            DbUpdateException dbEx when IsAttendanceSchemaError(dbEx) =>
                (HttpStatusCode.Conflict, "La base de datos no tiene aplicada la actualizacion de asistencia. Debe crear la tabla Ingresos y la columna Memberships.IngresosUtilizados."),
            _ => (HttpStatusCode.InternalServerError, "Ocurrio un error interno en el servidor.")
        };

        if ((int)statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception");
            await LogInternalServerErrorAsync(context, exception);
        }
        else
        {
            _logger.LogWarning("Handled exception: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)statusCode
        });

        await context.Response.WriteAsync(response);
    }

    /// <summary>
    /// Registra las excepciones de error interno (500) en la base de datos y rota los logs manteniendo un tope de 100 registros.
    /// </summary>
    private async Task LogInternalServerErrorAsync(HttpContext context, Exception exception)
    {
        try
        {
            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            dbContext.ErrorLogs.Add(new ErrorLog
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Path = context.Request.Path.Value,
                Method = context.Request.Method,
                Timestamp = DateTime.Now.AddHours(-3)
            });

            await dbContext.SaveChangesAsync();

            var oldestToKeep = await dbContext.ErrorLogs
                .OrderByDescending(errorLog => errorLog.Id)
                .Skip(99)
                .Select(errorLog => errorLog.Id)
                .FirstOrDefaultAsync();

            if (oldestToKeep > 0)
            {
                await dbContext.ErrorLogs
                    .Where(errorLog => errorLog.Id < oldestToKeep)
                    .ExecuteDeleteAsync();
            }
        }
        catch (Exception dbException)
        {
            _logger.LogError(dbException, "Failed to log exception to database");
        }
    }

    /// <summary>
    /// Determina si un error de base de datos se debe a que falta la migración/esquema del módulo de asistencia.
    /// </summary>
    private static bool IsAttendanceSchemaError(Exception exception)
    {
        var message = $"{exception.Message} {exception.InnerException?.Message}".ToLowerInvariant();

        return
            (message.Contains("ingresos") && (message.Contains("relation") || message.Contains("table") || message.Contains("no such table") || message.Contains("relación") || message.Contains("relacion") || message.Contains("tabla"))) ||
            (message.Contains("ingresosutilizados") && (message.Contains("column") || message.Contains("no such column") || message.Contains("columna"))) ||
            (message.Contains("memberships") && message.Contains("ingresosutilizados"));
    }
}