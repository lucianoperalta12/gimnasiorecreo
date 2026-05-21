using GymAdmin.Application.DTOs.Ingresos;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class IngresoService : IIngresoService
{
    private readonly AppDbContext _context;

    public IngresoService(AppDbContext context) => _context = context;

    public async Task<RegistrarIngresoResponse> RegistrarAsync(int terminalUserId, RegistrarIngresoRequest request)
    {
        var terminal = await _context.Users
            .Include(x => x.Gym)
            .FirstOrDefaultAsync(x => x.Id == terminalUserId)
            ?? throw new UnauthorizedAccessException("Usuario inválido.");

        if (terminal.Rol != UserRole.Terminal)
            throw new UnauthorizedAccessException("No autorizado.");

        var dni = request.Dni.Trim();
        if (string.IsNullOrWhiteSpace(dni))
            throw new ArgumentException("Debe ingresar un DNI.");

        var alumno = await _context.Users
            .FirstOrDefaultAsync(x => x.Dni == dni && x.Rol == UserRole.Alumno && x.Activo)
            ?? throw new InvalidOperationException("No existe un alumno activo con ese DNI.");

        if (alumno.GymId != terminal.GymId)
            throw new InvalidOperationException("El alumno no pertenece al gimnasio de esta terminal.");

        var membership = await _context.Memberships
            .Include(x => x.Plan)
            .Include(x => x.Gym)
            .Where(x => x.AlumnoId == alumno.Id && x.GymId == terminal.GymId && x.Estado == MembershipStatus.Activa)
            .OrderByDescending(x => x.FechaCreacion)
            .FirstOrDefaultAsync();

        if (membership is null)
            throw new InvalidOperationException("El alumno no tiene una membresía activa.");

        if (membership.FechaVencimiento < DateTime.UtcNow)
            throw new InvalidOperationException("La membresía del alumno está vencida.");

        if (!membership.Plan.PaseLibre)
        {
            var ingresosDisponibles = CalcularIngresosDisponibles(membership);
            if (membership.IngresosUtilizados >= ingresosDisponibles)
                throw new InvalidOperationException("El alumno no posee ingresos disponibles según su membresía.");

            if (membership.Plan.DiasPorSemana.HasValue && membership.Plan.DiasPorSemana.Value > 0)
            {
                var today = DateTime.UtcNow.Date;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
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
            GymId = terminal.GymId,
            AlumnoId = alumno.Id,
            TerminalId = terminal.Id,
            MembershipId = membership.Id,
            FechaHora = DateTime.UtcNow
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
            $"{terminal.Nombre} {terminal.Apellido}".Trim(),
            membership.Plan.Nombre,
            membership.Plan.PaseLibre,
            membership.IngresosUtilizados
        );
    }

    public async Task<List<IngresoListItemDto>> GetAllAsync(int requesterId, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null, int? alumnoId = null, int? gymId = null)
    {
        var requester = await _context.Users.FindAsync(requesterId)
            ?? throw new UnauthorizedAccessException("Usuario inválido.");

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
            var desde = fechaDesde ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var hasta = fechaHasta ?? desde;
            var inicio = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var fin = DateTime.SpecifyKind(hasta.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
            query = query.Where(x => x.FechaHora >= inicio && x.FechaHora <= fin);
        }
        else if (!alumnoId.HasValue)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var inicio = DateTime.SpecifyKind(hoy.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var fin = inicio.AddDays(1);
            query = query.Where(x => x.FechaHora >= inicio && x.FechaHora < fin);
        }

        return await query
            .OrderByDescending(x => x.FechaHora)
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
}
