using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Ingresos;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class IngresoService : IIngresoService
{
    private readonly AppDbContext _context;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public IngresoService(AppDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RegistrarIngresoResponse> RegistrarAsync(int terminalUserId, RegistrarIngresoRequest request)
    {
        var terminal = await GetRequesterAsync(terminalUserId);
        var terminalEntity = await _context.Users
            .Include(x => x.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(x => x.Id == terminal.Id)
            ?? throw new UnauthorizedAccessException("Usuario inválido.");

        if (terminal.Rol is not (UserRole.Terminal or UserRole.Administrativo or UserRole.Superusuario))
            throw new UnauthorizedAccessException("No autorizado.");

        var terminalAssociation = terminalEntity.GymUsers.FirstOrDefault(gu => gu.GymId == terminal.GymId && gu.Activo && gu.Gym.Activo)
            ?? throw new UnauthorizedAccessException("La terminal no está asociada al gimnasio activo.");

        var dni = request.Dni.Trim();
        if (string.IsNullOrWhiteSpace(dni))
            throw new ArgumentException("Debe ingresar un DNI.");

        var alumno = await _context.Users
            .Include(x => x.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(x =>
                x.Dni == dni &&
                x.Activo &&
                x.GymUsers.Any(gu => gu.GymId == terminalAssociation.GymId && gu.Rol == UserRole.Alumno && gu.Activo))
            ?? throw new InvalidOperationException("No existe un alumno activo con ese DNI.");

        var alumnoAssociation = alumno.GymUsers.FirstOrDefault(gu => gu.GymId == terminalAssociation.GymId && gu.Rol == UserRole.Alumno && gu.Activo)
            ?? throw new InvalidOperationException("El alumno no pertenece al gimnasio de esta terminal.");

        var membership = await _context.Memberships
            .Include(x => x.Plan)
            .Include(x => x.Gym)
            .Where(x => x.AlumnoId == alumno.Id && x.GymId == terminalAssociation.GymId && x.Estado == MembershipStatus.Activa)
            .OrderByDescending(x => x.FechaCreacion)
            .FirstOrDefaultAsync();

        if (membership is null)
            throw new InvalidOperationException("El alumno no tiene una membresía activa.");

        if (membership.FechaVencimiento < DateTime.Now)
            throw new InvalidOperationException("La membresía del alumno está vencida.");

        if (!membership.Plan.PaseLibre)
        {
            var ingresosDisponibles = CalcularIngresosDisponibles(membership);
            if (membership.IngresosUtilizados >= ingresosDisponibles)
                throw new InvalidOperationException("El alumno no posee ingresos disponibles según su membresía.");

            if (membership.Plan.DiasPorSemana.HasValue && membership.Plan.DiasPorSemana.Value > 0)
            {
                var today = DateTime.Now.Date;
                var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var startOfWeek = today.AddDays(-1 * diff);

                var ingresosEstaSemana = await _context.Ingresos
                    .CountAsync(x => x.AlumnoId == alumno.Id
                                  && x.MembershipId == membership.Id
                                  && x.FechaHora >= startOfWeek);

                if (ingresosEstaSemana >= membership.Plan.DiasPorSemana.Value)
                    throw new InvalidOperationException($"El alumno ya superó el límite de {membership.Plan.DiasPorSemana.Value} ingresos para esta semana.");
            }
        }

        var ingreso = new Ingreso
        {
            GymId = terminalAssociation.GymId,
            AlumnoId = alumno.Id,
            TerminalId = terminalEntity.Id,
            MembershipId = membership.Id,
            FechaHora = DateTime.Now
        };

        if (!membership.Plan.PaseLibre)
            membership.IngresosUtilizados += 1;

        _context.Ingresos.Add(ingreso);
        await _context.SaveChangesAsync();

        return new RegistrarIngresoResponse(
            ingreso.Id,
            alumno.Id,
            $"{alumno.Nombre} {alumno.Apellido}".Trim(),
            alumno.Dni,
            membership.Gym.Nombre,
            ingreso.FechaHora,
            $"{terminalEntity.Nombre} {terminalEntity.Apellido}".Trim(),
            membership.Plan.Nombre,
            membership.Plan.PaseLibre,
            membership.IngresosUtilizados,
            membership.FechaVencimiento
        );
    }

    public async Task<PagedResult<IngresoListItemDto>> GetAllAsync(int requesterId, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null, int? alumnoId = null, int? gymId = null, int? page = null, int? pageSize = null)
    {
        var requester = await GetRequesterAsync(requesterId);

        if (requester.Rol is not (UserRole.Superusuario or UserRole.Administrativo))
            throw new UnauthorizedAccessException("No autorizado.");

        var query = _context.Ingresos
            .AsNoTracking()
            .Include(x => x.Alumno)
            .Include(x => x.Gym)
            .Include(x => x.Terminal)
            .Include(x => x.Membership)
                .ThenInclude(x => x.Plan)
            .AsQueryable();

        if (requester.Rol != UserRole.Superusuario)
            query = query.Where(x => x.GymId == requester.GymId);
        else if (gymId.HasValue)
            query = query.Where(x => x.GymId == gymId.Value);

        if (alumnoId.HasValue)
        {
            query = query.Where(x => x.AlumnoId == alumnoId.Value);
        }

        if (fechaDesde.HasValue || fechaHasta.HasValue)
        {
            var desde = fechaDesde ?? DateOnly.FromDateTime(DateTime.Now);
            var hasta = fechaHasta ?? desde;
            var (inicio, fin) = GetLocalDateRangeAsUtc(desde, hasta);
            query = query.Where(x => x.FechaHora >= inicio && x.FechaHora < fin);
        }
        else if (!alumnoId.HasValue)
        {
            var argentinaTimeZone = GetArgentinaTimeZone();
            var hoy = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, argentinaTimeZone));
            var (inicio, fin) = GetLocalDateRangeAsUtc(hoy, hoy);
            query = query.Where(x => x.FechaHora >= inicio && x.FechaHora < fin);
        }

        var totalCount = await query.CountAsync();
        var pagedQuery = ApplyPagination(query.OrderByDescending(x => x.FechaHora), page, pageSize);

        var items = await pagedQuery
            .Select(x => new IngresoListItemDto(
                x.Id,
                (x.Alumno.Nombre + " " + x.Alumno.Apellido).Trim(),
                x.Alumno.Dni,
                x.Gym.Nombre,
                x.FechaHora,
                (x.Terminal.Nombre + " " + x.Terminal.Apellido).Trim(),
                x.Membership.Plan.Nombre
            ))
            .ToListAsync();

        return new PagedResult<IngresoListItemDto>(items, totalCount, page, NormalizePageSize(pageSize));
    }

    public async Task<List<IngresoHoyItemDto>> GetTodayAsync(int requesterId, int? gymId = null)
    {
        var requester = await GetRequesterAsync(requesterId);

        if (requester.Rol is not (UserRole.Superusuario or UserRole.Administrativo))
            throw new UnauthorizedAccessException("No autorizado.");

        var query = _context.Ingresos
            .AsNoTracking()
            .AsQueryable();

        if (requester.Rol != UserRole.Superusuario)
            query = query.Where(x => x.GymId == requester.GymId);
        else if (gymId.HasValue)
            query = query.Where(x => x.GymId == gymId.Value);

        var argentinaTimeZone = GetArgentinaTimeZone();
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, argentinaTimeZone));
        var (inicio, fin) = GetLocalDateRangeAsUtc(today, today);

        return await query
            .Where(x => x.FechaHora >= inicio && x.FechaHora < fin)
            .OrderByDescending(x => x.FechaHora)
            .Select(x => new IngresoHoyItemDto(
                x.Id,
                (x.Alumno.Nombre + " " + x.Alumno.Apellido).Trim(),
                x.Membership.Plan.Nombre,
                x.FechaHora
            ))
            .ToListAsync();
    }

    private static (DateTime Inicio, DateTime Fin) GetLocalDateRangeAsUtc(DateOnly desde, DateOnly hasta)
    {
        var argentinaTimeZone = GetArgentinaTimeZone();
        var inicioLocal = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        var finLocal = DateTime.SpecifyKind(hasta.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return (
            TimeZoneInfo.ConvertTimeToUtc(inicioLocal, argentinaTimeZone),
            TimeZoneInfo.ConvertTimeToUtc(finLocal, argentinaTimeZone)
        );
    }

    private static TimeZoneInfo GetArgentinaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        }
    }

    private static int CalcularIngresosDisponibles(Membership membership)
    {
        if (membership.Plan.PaseLibre) return int.MaxValue;
        if (membership.Plan.DiasPorSemana is null or <= 0)
            throw new InvalidOperationException("La membresía no tiene una cantidad válida de ingresos.");

        var dias = Math.Max(1, (membership.FechaVencimiento.Date - membership.FechaInicio.Date).Days);
        var semanas = (int)Math.Ceiling(dias / 7d);
        return semanas * membership.Plan.DiasPorSemana.Value;
    }

    private async Task<User> GetRequesterAsync(int requesterId)
    {
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Id == requesterId)
            ?? throw new UnauthorizedAccessException("Usuario inválido.");

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var gymIdClaim = httpContext.User.FindFirst("gymId")?.Value;
            if (int.TryParse(gymIdClaim, out var gymId))
            {
                user.GymId = gymId;
            }

            var activeAssociation = user.GymUsers.FirstOrDefault(gu => gu.GymId == user.GymId && gu.Activo) ?? user.GymUsers.FirstOrDefault(gu => gu.Activo);
            if (activeAssociation != null)
            {
                user.Rol = activeAssociation.Rol;
                if (user.GymId == 0)
                {
                    user.GymId = activeAssociation.GymId;
                }
            }
        }

        return user;
    }

    private static IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int? page, int? pageSize)
    {
        var normalizedPageSize = NormalizePageSize(pageSize);
        if (!normalizedPageSize.HasValue)
            return query;

        var normalizedPage = NormalizePage(page);
        return query.Skip((normalizedPage - 1) * normalizedPageSize.Value).Take(normalizedPageSize.Value);
    }

    private static int NormalizePage(int? page) => page.GetValueOrDefault(1) > 0 ? page.GetValueOrDefault(1) : 1;

    private static int? NormalizePageSize(int? pageSize)
    {
        if (!pageSize.HasValue || pageSize.Value <= 0)
            return null;

        return Math.Min(pageSize.Value, 200);
    }
}
