using System.Data.Common;
using System.Net;
using System.Text.Json;
using GymAdmin.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Api.Middleware;

/// <summary>
/// Middleware global para la captura y manejo unificado de excepciones en la API.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
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
            await ErrorLogHelper.LogAsync(
                _scopeFactory,
                exception,
                context.Request.Path.Value ?? string.Empty,
                context.Request.Method);
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