using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Memberships;
using GymAdmin.Application.Helpers;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymAdmin.Application.Services;

public class MembershipService : IMembershipService
{
    private readonly AppDbContext _context;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;

    public MembershipService(
        AppDbContext context,
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _scopeFactory = scopeFactory;
    }

    public async Task<PagedResult<MembershipListDto>> GetAllAsync(
        int requesterId,
        int? gymId = null,
        int? alumnoId = null,
        string? estado = null,
        string? search = null,
        string? sortBy = null,
        bool? sortDesc = null,
        int? page = null,
        int? pageSize = null,
        DateTime? fechaVencimientoDesde = null,
        DateTime? fechaVencimientoHasta = null,
        bool? sinActiva = null)
    {
        var requester = await GetRequesterAsync(requesterId);

        var query = BuildMembershipQuery(requester, gymId, alumnoId, estado, search, sortBy, sortDesc, fechaVencimientoDesde, fechaVencimientoHasta, sinActiva);
        var totalCount = await query.CountAsync();
        query = ApplyPagination(query, page, pageSize);
        var items = await query.ToListAsync();
        var dtos = await MapToListDtosAsync(items);

        return new PagedResult<MembershipListDto>(dtos, totalCount, page, NormalizePageSize(pageSize));
    }

    public async Task<MembershipDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        var membership = await LoadMembershipAsync(id);
        if (membership is null) return null;

        EnsureCanViewMembership(requester, membership);

        return await MapToDtoAsync(membership);
    }

    public async Task<List<MembershipListDto>> GetByStudentIdAsync(int requesterId, int studentId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var student = await GetStudentAsync(studentId);
        EnsureCanViewStudent(requester, student);

        var items = await _context.Memberships
            .AsNoTracking()
            .Include(m => m.Alumno)
            .Include(m => m.Plan)
            .Where(m => m.AlumnoId == studentId && m.GymId == student.GymId)
            .OrderByDescending(m => m.FechaCreacion)
            .ToListAsync();

        return await MapToListDtosAsync(items);
    }

    public async Task<StudentAccessDto> GetStudentAccessAsync(int requesterId, int studentId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var student = await GetStudentAsync(studentId);
        EnsureCanViewStudent(requester, student);
        return await BuildStudentAccessDtoAsync(student);
    }

    public async Task<StudentAccessDto> GetMyAccessAsync(int requesterId)
    {
        var requester = await GetRequesterAsync(requesterId);
        if (requester.Rol != UserRole.Alumno)
            throw new UnauthorizedAccessException("Solo disponible para alumnos.");

        return await BuildStudentAccessDtoAsync(requester);
    }

    public async Task<MembershipDto> CreateAsync(int requesterId, CreateMembershipRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManageMemberships(requester);

        var student = await GetStudentAsync(request.AlumnoId);
        EnsureSameGym(requester, student.GymId);

        var plan = await GetActivePlanAsync(request.PlanId, student.GymId);

        if (await HasActiveMembershipAsync(student.Id))
            throw new InvalidOperationException("El alumno ya tiene una membresía activa. Use renovación para extender.");

        var argentina = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        var fechaInicio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentina);
        var membership = new Membership
        {
            GymId = student.GymId,
            AlumnoId = student.Id,
            PlanId = plan.Id,
            FechaInicio = fechaInicio,
            FechaVencimiento = fechaInicio.AddDays(plan.DuracionDias),
            Estado = MembershipStatus.Activa,
            Notas = request.Notas?.Trim()
        };

        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(requesterId, membership.Id))!;
    }

    public async Task<MembershipDto> RenewAsync(int requesterId, int studentId, RenewMembershipRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManageMemberships(requester);

        var student = await GetStudentAsync(studentId);
        EnsureSameGym(requester, student.GymId);

        var plan = await GetActivePlanAsync(request.PlanId, student.GymId);
        await CloseActiveMembershipsAsync(student.Id);

        var argentina = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        var fechaInicio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentina);
        var membership = new Membership
        {
            GymId = student.GymId,
            AlumnoId = student.Id,
            PlanId = plan.Id,
            FechaInicio = fechaInicio,
            FechaVencimiento = fechaInicio.AddDays(plan.DuracionDias),
            Estado = MembershipStatus.Activa,
            Notas = request.Notas?.Trim()
        };

        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(requesterId, membership.Id))!;
    }

    public async Task<MembershipDto> CancelAsync(int requesterId, int id, CancelMembershipRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManageMemberships(requester);

        var membership = await _context.Memberships
            .Include(m => m.Alumno)
            .Include(m => m.Plan)
            .Include(m => m.Gym)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException("Membresía no encontrada.");

        EnsureSameGym(requester, membership.GymId);

        if (membership.Estado != MembershipStatus.Activa)
            throw new InvalidOperationException("Solo se pueden cancelar membresías activas.");

        membership.Estado = MembershipStatus.Cancelada;
        if (!string.IsNullOrWhiteSpace(request.Motivo))
        {
            var motivo = request.Motivo.Trim();
            membership.Notas = string.IsNullOrWhiteSpace(membership.Notas)
                ? $"Cancelada: {motivo}"
                : $"{membership.Notas} | Cancelada: {motivo}";
        }

        await _context.SaveChangesAsync();
        return await MapToDtoAsync(membership);
    }

    public async Task<DashboardMembershipSummaryDto> GetDashboardSummaryAsync(int requesterId, int? gymId)
    {
        var requester = await GetRequesterAsync(requesterId);

        var effectiveGymId = requester.Rol == UserRole.Superusuario
            ? gymId
            : (int?)requester.GymId;

        var argentina = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentina);
        var limite7 = now.AddDays(7).AddDays(1).AddTicks(-1);
        var haceUnMes = now.AddMonths(-1);
        var finHoy = now.AddDays(1).AddTicks(-1);

        // Query 1: Activas que vencen en los próximos 7 días
        var porVencerQuery = _context.Memberships
            .AsNoTracking()
            .Where(m => m.Estado == MembershipStatus.Activa
                     && m.FechaVencimiento >= now
                     && m.FechaVencimiento <= limite7);

        if (effectiveGymId.HasValue)
            porVencerQuery = porVencerQuery.Where(m => m.GymId == effectiveGymId.Value);

        var porVencer = await porVencerQuery
            .OrderBy(m => m.FechaVencimiento)
            .Select(m => new DashboardMembershipItemDto(
                m.Id,
                m.AlumnoId,
                (m.Alumno.Nombre + " " + m.Alumno.Apellido).Trim(),
                m.Alumno.Dni,
                m.Alumno.Telefono,
                m.Alumno.Email,
                m.Plan.Nombre,
                m.FechaVencimiento))
            .ToListAsync();

        // Query 2: Vencidas en el último mes, sin membresía activa vigente
        var alumnosConActiva = _context.Memberships
            .Where(m => m.Estado == MembershipStatus.Activa)
            .Select(m => m.AlumnoId);

        var vencidasQuery = _context.Memberships
            .AsNoTracking()
            .Where(m => m.Estado == MembershipStatus.Vencida
                     && m.FechaVencimiento >= haceUnMes
                     && m.FechaVencimiento <= finHoy
                     && !alumnosConActiva.Contains(m.AlumnoId));

        if (effectiveGymId.HasValue)
            vencidasQuery = vencidasQuery.Where(m => m.GymId == effectiveGymId.Value);

        var vencidasRaw = await vencidasQuery
            .Select(m => new DashboardMembershipItemDto(
                m.Id,
                m.AlumnoId,
                (m.Alumno.Nombre + " " + m.Alumno.Apellido).Trim(),
                m.Alumno.Dni,
                m.Alumno.Telefono,
                m.Alumno.Email,
                m.Plan.Nombre,
                m.FechaVencimiento))
            .ToListAsync();

        // Deduplicar por alumno: queda la membresía más reciente
        var vencidas = vencidasRaw
            .GroupBy(m => m.AlumnoId)
            .Select(g => g.OrderByDescending(m => m.FechaVencimiento).First())
            .OrderByDescending(m => m.FechaVencimiento)
            .ToList();

        return new DashboardMembershipSummaryDto(porVencer, vencidas);
    }

    private IQueryable<Membership> BuildMembershipQuery(
        User requester,
        int? gymId,
        int? alumnoId,
        string? estado,
        string? search,
        string? sortBy,
        bool? sortDesc,
        DateTime? fechaVencimientoDesde = null,
        DateTime? fechaVencimientoHasta = null,
        bool? sinActiva = null)
    {
        var query = _context.Memberships
            .AsNoTracking()
            .Include(m => m.Alumno)
            .Include(m => m.Plan)
            .AsQueryable();

        if (requester.Rol == UserRole.Superusuario)
        {
            if (gymId.HasValue && gymId.Value > 0)
                query = query.Where(m => m.GymId == gymId.Value);
        }
        else
        {
            query = query.Where(m => m.GymId == requester.GymId);
        }

        if (alumnoId.HasValue)
            query = query.Where(m => m.AlumnoId == alumnoId.Value);

        if (!string.IsNullOrWhiteSpace(estado)
            && Enum.TryParse<MembershipStatus>(estado, true, out var status))
        {
            query = query.Where(m => m.Estado == status);
        }

        if (fechaVencimientoDesde.HasValue)
        {
            var desde = fechaVencimientoDesde.Value.Date;
            query = query.Where(m => m.FechaVencimiento >= desde);
        }

        if (fechaVencimientoHasta.HasValue)
        {
            var hasta = fechaVencimientoHasta.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(m => m.FechaVencimiento <= hasta);
        }

        if (sinActiva == true)
        {
            var alumnosConActiva = _context.Memberships
                .Where(m => m.Estado == MembershipStatus.Activa)
                .Select(m => m.AlumnoId);
            query = query.Where(m => !alumnosConActiva.Contains(m.AlumnoId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(m =>
                (m.Alumno.Nombre + " " + m.Alumno.Apellido).ToLower().Contains(normalizedSearch) ||
                m.Plan.Nombre.ToLower().Contains(normalizedSearch));
        }

        return ApplySorting(query, sortBy, sortDesc);
    }

    private async Task<StudentAccessDto> BuildStudentAccessDtoAsync(User student)
    {
        var gym = await _context.Gyms.FindAsync(student.GymId);

        var active = await _context.Memberships
            .AsNoTracking()
            .Include(m => m.Plan)
            .Where(m => m.AlumnoId == student.Id && m.GymId == student.GymId && m.Estado == MembershipStatus.Activa)
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var pending = active is not null && await HasPendingPaymentsAsync(active.Id);
        var access = AccessStatusHelper.DeriveFromMembership(active, pending);

        return new StudentAccessDto(
            student.Id,
            student.Nombre,
            student.Apellido,
            student.Email,
            student.GymId,
            AccessStatusHelper.ToDisplayString(access),
            active?.Id,
            active?.Plan.Nombre,
            active?.FechaVencimiento,
            active is not null ? AccessStatusHelper.DiasRestantes(active) : null,
            gym?.Moneda
        );
    }

    private async Task<List<MembershipListDto>> MapToListDtosAsync(List<Membership> items)
    {
        var activeMembershipIds = items
            .Where(m => m.Estado == MembershipStatus.Activa)
            .Select(m => m.Id)
            .ToList();

        var alumnoIds = items
            .Select(m => m.AlumnoId)
            .Distinct()
            .ToList();

        var pendingMembershipIds = activeMembershipIds.Count == 0
            ? []
            : await _context.MembershipPayments
                .AsNoTracking()
                .Where(p => activeMembershipIds.Contains(p.MembresiaId) && p.Estado == PaymentStatus.Pendiente)
                .Select(p => p.MembresiaId)
                .Distinct()
                .ToListAsync();

        var studentMemberships = alumnoIds.Count == 0
            ? []
            : await _context.Memberships
                .AsNoTracking()
                .Where(m => alumnoIds.Contains(m.AlumnoId) &&
                            (m.Estado == MembershipStatus.Activa || m.Estado == MembershipStatus.Vencida))
                .Select(m => new StudentMembershipRenewalCandidate(
                    m.Id,
                    m.AlumnoId,
                    m.Estado,
                    m.FechaVencimiento))
                .ToListAsync();

        var renewalEligibilityByMembershipId = BuildRenewalEligibilityLookup(studentMemberships);
        var pendingLookup = pendingMembershipIds.ToHashSet();
        var result = new List<MembershipListDto>();
        foreach (var m in items)
        {
            var pending = m.Estado == MembershipStatus.Activa && pendingLookup.Contains(m.Id);
            var access = AccessStatusHelper.DeriveFromMembership(m, pending);
            var puedeRenovar = renewalEligibilityByMembershipId.TryGetValue(m.Id, out var renewAllowed) && renewAllowed;
            result.Add(new MembershipListDto(
                m.Id,
                m.GymId,
                m.AlumnoId,
                $"{m.Alumno.Nombre} {m.Alumno.Apellido}".Trim(),
                m.Alumno.Dni,
                m.Plan.Nombre,
                m.FechaInicio,
                m.FechaVencimiento,
                m.Estado.ToString(),
                AccessStatusHelper.ToDisplayString(access),
                AccessStatusHelper.DiasRestantes(m),
                m.IngresosUtilizados,
                m.Plan.PaseLibre,
                m.Plan.DiasPorSemana,
                puedeRenovar,
                m.Alumno.Telefono
            ));
        }
        return result;
    }

    private async Task<MembershipDto> MapToDtoAsync(Membership m)
    {
        if (m.Alumno is null) await _context.Entry(m).Reference(x => x.Alumno).LoadAsync();
        if (m.Plan is null) await _context.Entry(m).Reference(x => x.Plan).LoadAsync();
        if (m.Gym is null) await _context.Entry(m).Reference(x => x.Gym).LoadAsync();

        if (m.Alumno is null || m.Plan is null)
            throw new InvalidOperationException("Datos de membresía incompletos.");

        var pending = m.Estado == MembershipStatus.Activa && await HasPendingPaymentsAsync(m.Id);
        var access = AccessStatusHelper.DeriveFromMembership(m, pending);

        return new MembershipDto(
            m.Id,
            m.GymId,
            m.Gym?.Nombre,
            m.AlumnoId,
            m.Alumno.Nombre,
            m.Alumno.Apellido,
            m.Alumno.Email,
            m.PlanId,
            m.Plan.Nombre,
            m.Plan.Precio,
            m.Plan.DuracionDias,
            m.FechaInicio,
            m.FechaVencimiento,
            m.Estado.ToString(),
            m.Notas,
            m.FechaCreacion,
            AccessStatusHelper.DiasRestantes(m),
            AccessStatusHelper.ToDisplayString(access),
            m.Gym?.Moneda
        );
    }

    private async Task<Membership?> LoadMembershipAsync(int id) =>
        await _context.Memberships
            .AsNoTracking()
            .Include(m => m.Alumno)
            .Include(m => m.Plan)
            .Include(m => m.Gym)
            .FirstOrDefaultAsync(m => m.Id == id);

    /// <summary>
    /// Punto unificado de expiración de membresías vencidas. Marca como <c>Vencida</c> toda membresía
    /// cuya <c>FechaVencimiento</c> ya pasó y que aún figure como <c>Activa</c>, luego envía el email
    /// de notificación a cada alumno afectado.
    /// </summary>
    /// <remarks>
    /// <para>El estado se persiste en base de datos <b>antes</b> de intentar el envío de emails,
    /// de modo que un fallo en el envío no deja la membresía sin expirar.</para>
    /// <para>Llamado en dos contextos distintos:</para>
    /// <list type="bullet">
    ///   <item><b>On-demand</b> — desde <c>CreateAsync</c> y <c>RenewAsync</c>, con <paramref name="requester"/>
    ///   y <paramref name="gymId"/> del gimnasio del alumno, para garantizar consistencia inmediata antes
    ///   de validar y guardar la nueva membresía.</item>
    ///   <item><b>Background</b> — desde <see cref="GymAdmin.Api.BackgroundServices.MembershipExpirationService"/>
    ///   cada 6 horas, con ambos parámetros en <c>null</c> para operar globalmente sobre todos los gimnasios.</item>
    /// </list>
    /// </remarks>
    /// <param name="requester">
    /// Usuario que origina la operación. Si es <c>null</c> o su rol es <c>Superusuario</c>, no se aplica
    /// filtro de gimnasio (salvo que <paramref name="gymId"/> sea un valor concreto).
    /// </param>
    /// <param name="gymId">
    /// Identificador del gimnasio sobre el que restringir la expiración. <c>null</c> indica sin restricción
    /// adicional (útil para la ejecución global del background service).
    /// </param>
    public async Task ExpireOverdueMembershipsAsync()
    {
        var argentina = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentina);

        var query = _context.Memberships
            .Include(m => m.Alumno)
            .Include(m => m.Gym)
          .Where(m => m.Estado == MembershipStatus.Activa &&
                m.FechaVencimiento <= now);

        var overdue = await query.ToListAsync();
        var querystring = query.ToQueryString();
        if (overdue.Count == 0) return;

        foreach (var m in overdue)
            m.Estado = MembershipStatus.Vencida;

        await _context.SaveChangesAsync();

        foreach (var m in overdue)
        {
            await SendExpirationEmailAsync(m);
            await Task.Delay(100, CancellationToken.None);
        }
    }

    public async Task SendExpirationEmailManualAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManageMemberships(requester);

        var m = await _context.Memberships
            .Include(m => m.Alumno)
            .Include(m => m.Gym)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (m == null)
            throw new KeyNotFoundException("Membresía no encontrada.");

        EnsureSameGym(requester, m.GymId);

        if (m.Alumno == null || string.IsNullOrWhiteSpace(m.Alumno.Email))
            throw new InvalidOperationException("El alumno no tiene un correo electrónico configurado.");

        if (!await EmailValidator.IsValidEmailAsync(m.Alumno.Email, _scopeFactory))
            throw new InvalidOperationException("El correo electrónico del alumno no es válido.");

        await SendExpirationEmailAsync(m, throwOnError: true);
    }

    public async Task SendExpirationEmailAsync(Membership m, bool throwOnError = false)
    {
        if (m.Alumno == null || string.IsNullOrWhiteSpace(m.Alumno.Email) ||
            !await EmailValidator.IsValidEmailAsync(m.Alumno.Email, _scopeFactory))
        {
            if (throwOnError)
                throw new InvalidOperationException("El alumno no tiene un correo electrónico válido.");
            return;
        }

        try
        {
            var studentName = m.Alumno.Nombre;
            var gymName = m.Gym?.Nombre ?? "el gimnasio";
            var gymColor = m.Gym?.ColorPrincipalHex ?? "#ff6600";

            var logoHtml = !string.IsNullOrWhiteSpace(m.Gym?.LogoUrl)
                ? $"<div style='text-align: center; margin-bottom: 20px;'><img src='{m.Gym.LogoUrl}' alt='{gymName}' style='max-height: 80px; border-radius: 8px;' /></div>"
                : "";
            logoHtml = logoHtml + $"<div style='font-size: 24px; font-weight: bold; color: {gymColor}; text-align: center; margin-bottom: 20px;'>{gymName}</div>";

            var body = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 40px 20px; color: #333333;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); overflow: hidden; border-top: 5px solid {gymColor};"">
        <div style=""padding: 30px; text-align: center;"">
            {logoHtml}
            <h2 style=""color: #2c3e50; margin-top: 10px; font-weight: 600;"">Aviso de Vencimiento</h2>
        </div>
        <div style=""padding: 0 40px 40px 40px; line-height: 1.6; font-size: 16px;"">
            <p>Hola <strong>{studentName}</strong>, ¿cómo estás?</p>
            <p>Te recordamos que tu cuota del gimnasio se encuentra vencida. Te pedimos que regularices el pago para mantener tu acceso activo y seguir disfrutando de las actividades del gimnasio.</p>
            <p style=""margin-top: 30px;"">Muchas gracias.</p>
        </div>
        <div style=""background-color: #2c3e50; color: #ffffff; padding: 20px; text-align: center; font-size: 12px;"">
            Este es un correo automático enviado por {gymName}. Por favor, no respondas a este mensaje.
        </div>
    </div>
</div>";

            await _emailService.SendEmailAsync(
                to: m.Alumno.Email,
                subject: "Vencimiento de cuota de gimnasio",
                body: body,
                tipo: TipoCorreo.VencimientoMembresia,
                nombre: m.Alumno.Nombre,
                apellido: m.Alumno.Apellido,
                dni: m.Alumno.Dni,
                gymId: m.GymId,
                from: "fitcenter.manager@gmail.com"
            );
        }
        catch (Exception ex)
        {
            if (throwOnError)
                throw new InvalidOperationException($"Error al enviar el correo: {ex.Message}", ex);
        }
    }

    private async Task CloseActiveMembershipsAsync(int alumnoId)
    {
        var actives = await _context.Memberships
            .Where(m => m.AlumnoId == alumnoId && m.Estado == MembershipStatus.Activa)
            .ToListAsync();

        foreach (var m in actives)
            m.Estado = MembershipStatus.Vencida;

        if (actives.Count > 0)
            await _context.SaveChangesAsync();
    }

    private async Task<bool> HasActiveMembershipAsync(int alumnoId) =>
        await _context.Memberships.AnyAsync(m =>
            m.AlumnoId == alumnoId && m.Estado == MembershipStatus.Activa);

    private async Task<bool> HasPendingPaymentsAsync(int membershipId) =>
        await _context.MembershipPayments.AnyAsync(p =>
            p.MembresiaId == membershipId && p.Estado == PaymentStatus.Pendiente);

    private async Task<MembershipPlan> GetActivePlanAsync(int planId, int gymId)
    {
        var plan = await _context.MembershipPlans.FindAsync(planId)
            ?? throw new KeyNotFoundException("Plan no encontrado.");

        if (plan.GymId != gymId)
            throw new InvalidOperationException("El plan no pertenece al gimnasio del alumno.");

        if (!plan.Activo)
            throw new InvalidOperationException("El plan no está activo.");

        return plan;
    }

    private async Task<User> GetStudentAsync(int studentId)
    {
        var student = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Id == studentId)
            ?? throw new KeyNotFoundException("Alumno no encontrado.");

        var gymId = GetCurrentGymId();
        var association = student.GymUsers.FirstOrDefault(gu => gu.GymId == gymId && gu.Activo)
            ?? student.GymUsers.FirstOrDefault(gu => gu.Activo);

        if (association != null)
        {
            student.GymId = association.GymId;
            student.Rol = association.Rol;
        }

        if (student.Rol != UserRole.Alumno)
            throw new InvalidOperationException("Solo se pueden gestionar membresías de alumnos.");

        return student;
    }

    private static DateTime NormalizeDate(DateTime date) =>
        date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)
            : date.ToUniversalTime().Date;

    private static void EnsureCanManageMemberships(User requester)
    {
        if (requester.Rol is not (UserRole.Superusuario or UserRole.Administrativo))
            throw new UnauthorizedAccessException("No autorizado.");
    }

    private static void EnsureCanViewMembership(User requester, Membership membership)
    {
        if (requester.Rol == UserRole.Alumno && requester.Id != membership.AlumnoId)
            throw new UnauthorizedAccessException("No autorizado.");

        if (requester.Rol is UserRole.Profesor or UserRole.Administrativo
            && requester.GymId != membership.GymId)
            throw new UnauthorizedAccessException("No autorizado.");

        if (requester.Rol != UserRole.Superusuario
            && requester.Rol != UserRole.Alumno
            && requester.GymId != membership.GymId)
            throw new UnauthorizedAccessException("No autorizado.");
    }

    private static void EnsureCanViewStudent(User requester, User student)
    {
        if (requester.Rol == UserRole.Alumno && requester.Id != student.Id)
            throw new UnauthorizedAccessException("No autorizado.");

        if (requester.Rol != UserRole.Superusuario && requester.GymId != student.GymId)
            throw new UnauthorizedAccessException("No autorizado.");
    }

    private static void EnsureSameGym(User requester, int gymId)
    {
        if (requester.Rol != UserRole.Superusuario && requester.GymId != gymId)
            throw new UnauthorizedAccessException("No autorizado.");
    }

    private async Task<User> GetRequesterAsync(int requesterId)
    {
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Id == requesterId)
            ?? throw new UnauthorizedAccessException("Usuario invalido.");

        var gymId = GetCurrentGymId();
        if (gymId.HasValue)
        {
            user.GymId = gymId.Value;
        }

        var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (Enum.TryParse<UserRole>(roleClaim, true, out var role))
        {
            user.Rol = role;
        }
        else
        {
            var association = user.GymUsers.FirstOrDefault(gu => gu.GymId == user.GymId && gu.Activo) ?? user.GymUsers.FirstOrDefault(gu => gu.Activo);
            if (association != null)
            {
                user.Rol = association.Rol;
                if (user.GymId == 0)
                {
                    user.GymId = association.GymId;
                }
            }
        }

        return user;
    }

    private int? GetCurrentGymId()
    {
        var gymIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("gymId")?.Value;
        if (int.TryParse(gymIdClaim, out var gymId))
        {
            return gymId;
        }

        return null;
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

    private static IQueryable<Membership> ApplySorting(IQueryable<Membership> query, string? sortBy, bool? sortDesc)
    {
        var descending = sortDesc ?? true;
        var normalizedSortBy = sortBy?.Trim();

        return normalizedSortBy switch
        {
            nameof(MembershipListDto.AlumnoNombreCompleto) => descending
                ? query.OrderByDescending(m => m.Alumno.Nombre).ThenByDescending(m => m.Alumno.Apellido).ThenByDescending(m => m.Id)
                : query.OrderBy(m => m.Alumno.Nombre).ThenBy(m => m.Alumno.Apellido).ThenBy(m => m.Id),
            nameof(MembershipListDto.FechaInicio) => descending
                ? query.OrderByDescending(m => m.FechaInicio).ThenByDescending(m => m.Id)
                : query.OrderBy(m => m.FechaInicio).ThenBy(m => m.Id),
            nameof(MembershipListDto.FechaVencimiento) => descending
                ? query.OrderByDescending(m => m.FechaVencimiento).ThenByDescending(m => m.Id)
                : query.OrderBy(m => m.FechaVencimiento).ThenBy(m => m.Id),
            _ => descending
                ? query.OrderByDescending(m => m.FechaCreacion).ThenByDescending(m => m.Id)
                : query.OrderBy(m => m.FechaCreacion).ThenBy(m => m.Id)
        };
    }

    private static Dictionary<int, bool> BuildRenewalEligibilityLookup(List<StudentMembershipRenewalCandidate> memberships)
    {
        var result = new Dictionary<int, bool>();

        foreach (var group in memberships.GroupBy(m => m.AlumnoId))
        {
            var hasActive = group.Any(m => m.Estado == MembershipStatus.Activa);
            var latestExpiredMembershipId = group
                .Where(m => m.Estado == MembershipStatus.Vencida)
                .OrderByDescending(m => m.FechaVencimiento)
                .ThenByDescending(m => m.Id)
                .Select(m => m.Id)
                .FirstOrDefault();

            foreach (var membership in group)
            {
                var canRenew = membership.Estado == MembershipStatus.Activa ||
                               (membership.Estado == MembershipStatus.Vencida &&
                                !hasActive &&
                                latestExpiredMembershipId == membership.Id);

                result[membership.Id] = canRenew;
            }
        }

        return result;
    }

    private sealed record StudentMembershipRenewalCandidate(
        int Id,
        int AlumnoId,
        MembershipStatus Estado,
        DateTime FechaVencimiento);

    public async Task<List<MembershipRenovationReportDto>> GetRenovationsReportAsync(int requesterId, int? gymId)
    {
        var requester = await GetRequesterAsync(requesterId);

        int effectiveGymId;
        if (requester.Rol == UserRole.Superusuario)
        {
            if (!gymId.HasValue)
                throw new InvalidOperationException("El Superusuario debe especificar un GymId.");
            effectiveGymId = gymId.Value;
        }
        else
        {
            effectiveGymId = requester.GymId;
        }

        var sql = $"""
            WITH historial AS (
                SELECT
                    m."AlumnoId",
                    COUNT(*) FILTER (
                        WHERE m."Estado" <> 'Cancelada'
                          AND EXISTS (
                              SELECT 1
                              FROM "MembershipPayments" mp
                              WHERE mp."MembresiaId" = m."Id"
                                AND mp."Estado" = 'Completado'
                          )
                    ) AS total_membresias,
                    GREATEST(
                        COUNT(*) FILTER (
                            WHERE m."Estado" <> 'Cancelada'
                              AND EXISTS (
                                  SELECT 1
                                  FROM "MembershipPayments" mp
                                  WHERE mp."MembresiaId" = m."Id"
                                    AND mp."Estado" = 'Completado'
                              )
                        ) - 1,
                        0
                    ) AS renovaciones,
                    MIN(m."FechaInicio") FILTER (
                        WHERE m."Estado" <> 'Cancelada'
                          AND EXISTS (
                              SELECT 1
                              FROM "MembershipPayments" mp
                              WHERE mp."MembresiaId" = m."Id"
                                AND mp."Estado" = 'Completado'
                          )
                    ) AS primera_membresia,
                    MAX(m."FechaInicio") FILTER (
                        WHERE m."Estado" <> 'Cancelada'
                          AND EXISTS (
                              SELECT 1
                              FROM "MembershipPayments" mp
                              WHERE mp."MembresiaId" = m."Id"
                                AND mp."Estado" = 'Completado'
                          )
                    ) AS ultima_renovacion
                FROM "Memberships" m
                GROUP BY m."AlumnoId"
            ),
            membresia_actual AS (
                SELECT DISTINCT ON (m."AlumnoId")
                    m."AlumnoId",
                    m."Estado",
                    m."FechaVencimiento",
                    mp."Nombre" AS plan_actual
                FROM "Memberships" m
                INNER JOIN "MembershipPlans" mp ON mp."Id" = m."PlanId"
                ORDER BY m."AlumnoId", m."FechaInicio" DESC
            )
            SELECT
                u."Id",
                u."Nombre",
                u."Apellido",
                u."Dni",
                gu."GymId",
                COALESCE(h.total_membresias, 0) AS total_membresias,
                COALESCE(h.renovaciones, 0) AS renovaciones,
                CASE WHEN COALESCE(h.renovaciones, 0) > 0 THEN 'Sí' ELSE 'No' END AS renovo,
                h.primera_membresia,
                h.ultima_renovacion,
                ma.plan_actual,
                ma."Estado" AS estado_actual,
                ma."FechaVencimiento",
                CASE
                    WHEN ma."Estado" = 'Activa'
                     AND ma."FechaVencimiento" BETWEEN NOW() AND NOW() + INTERVAL '7 days'
                    THEN 'VENCE PRONTO'
                    ELSE NULL
                END AS alerta
            FROM "Users" u
            INNER JOIN "GymUsers" gu ON gu."UserId" = u."Id" AND gu."Activo" = TRUE
            LEFT JOIN historial h ON h."AlumnoId" = u."Id"
            LEFT JOIN membresia_actual ma ON ma."AlumnoId" = u."Id"
            WHERE u."Activo" = TRUE
              AND gu."GymId" = {effectiveGymId}
              AND COALESCE(h.renovaciones, 0) > 0
            ORDER BY renovaciones DESC, total_membresias DESC, u."Apellido", u."Nombre"
            LIMIT 20
            """;

        var raw = await _context.Database
            .SqlQueryRaw<RenovationRawRow>(sql)
            .ToListAsync();

        return raw.Select(r => new MembershipRenovationReportDto(
            r.Id,
            r.Nombre,
            r.Apellido,
            r.Dni,
            r.GymId,
            r.total_membresias,
            r.renovaciones,
            r.renovo,
            r.primera_membresia,
            r.ultima_renovacion,
            r.plan_actual,
            r.estado_actual,
            r.FechaVencimiento,
            r.alerta
        )).ToList();
    }

    private sealed class RenovationRawRow
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Dni { get; set; } = "";
        public int GymId { get; set; }
        public int total_membresias { get; set; }
        public int renovaciones { get; set; }
        public string renovo { get; set; } = "";
        public DateTime? primera_membresia { get; set; }
        public DateTime? ultima_renovacion { get; set; }
        public string? plan_actual { get; set; }
        public string? estado_actual { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? alerta { get; set; }
    }
}