using GymAdmin.Application.DTOs.Memberships;
using GymAdmin.Application.Helpers;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class MembershipService : IMembershipService
{
    private readonly AppDbContext _context;

    public MembershipService(AppDbContext context) => _context = context;

    public async Task<List<MembershipListDto>> GetAllAsync(
        int requesterId,
        int? gymId = null,
        int? alumnoId = null,
        string? estado = null)
    {
        var requester = await GetRequesterAsync(requesterId);
        await ExpireOverdueMembershipsAsync(requester, gymId);

        var query = BuildMembershipQuery(requester, gymId, alumnoId, estado);
        var items = await query.ToListAsync();

        return await MapToListDtosAsync(items);
    }

    public async Task<MembershipDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        var membership = await LoadMembershipAsync(id);
        if (membership is null) return null;

        EnsureCanViewMembership(requester, membership);
        await ExpireOverdueMembershipsAsync(requester, membership.GymId);

        membership = await LoadMembershipAsync(id);
        return membership is null ? null : await MapToDtoAsync(membership);
    }

    public async Task<List<MembershipListDto>> GetByStudentIdAsync(int requesterId, int studentId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var student = await GetStudentAsync(studentId);
        EnsureCanViewStudent(requester, student);
        await ExpireOverdueMembershipsAsync(requester, student.GymId);

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
        await ExpireOverdueMembershipsAsync(requester, student.GymId);

        if (await HasActiveMembershipAsync(student.Id))
            throw new InvalidOperationException("El alumno ya tiene una membresía activa. Use renovación para extender.");

        var fechaInicio = NormalizeDate(request.FechaInicio);
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
        await ExpireOverdueMembershipsAsync(requester, student.GymId);
        await CloseActiveMembershipsAsync(student.Id);

        var fechaInicio = NormalizeDate(request.FechaInicio ?? DateTime.UtcNow);
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

    private IQueryable<Membership> BuildMembershipQuery(
        User requester,
        int? gymId,
        int? alumnoId,
        string? estado)
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

        return query.OrderByDescending(m => m.FechaCreacion);
    }

    private async Task<StudentAccessDto> BuildStudentAccessDtoAsync(User student)
    {
        if (student.Gym is null) await _context.Entry(student).Reference(x => x.Gym).LoadAsync();
        await ExpireOverdueMembershipsAsync(null, student.GymId);

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
            student.Gym?.Moneda
        );
    }

    private async Task<List<MembershipListDto>> MapToListDtosAsync(List<Membership> items)
    {
        var result = new List<MembershipListDto>();
        foreach (var m in items)
        {
            var pending = m.Estado == MembershipStatus.Activa && await HasPendingPaymentsAsync(m.Id);
            var access = AccessStatusHelper.DeriveFromMembership(m, pending);
            result.Add(new MembershipListDto(
                m.Id,
                m.GymId,
                m.AlumnoId,
                $"{m.Alumno.Nombre} {m.Alumno.Apellido}".Trim(),
                m.Plan.Nombre,
                m.FechaInicio,
                m.FechaVencimiento,
                m.Estado.ToString(),
                AccessStatusHelper.ToDisplayString(access),
                AccessStatusHelper.DiasRestantes(m),
                m.Plan.PaseLibre,
                m.Plan.DiasPorSemana
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

    private async Task ExpireOverdueMembershipsAsync(User? requester, int? gymId)
    {
        var now = DateTime.UtcNow;
        var query = _context.Memberships
            .Where(m => m.Estado == MembershipStatus.Activa && m.FechaVencimiento < now);

        if (requester?.Rol != UserRole.Superusuario)
        {
            var filterGymId = gymId ?? requester?.GymId;
            if (filterGymId.HasValue)
                query = query.Where(m => m.GymId == filterGymId.Value);
        }
        else if (gymId.HasValue && gymId.Value > 0)
        {
            query = query.Where(m => m.GymId == gymId.Value);
        }

        var overdue = await query.ToListAsync();
        if (overdue.Count == 0) return;

        foreach (var m in overdue)
            m.Estado = MembershipStatus.Vencida;

        await _context.SaveChangesAsync();
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
        var student = await _context.Users.FindAsync(studentId)
            ?? throw new KeyNotFoundException("Alumno no encontrado.");

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

    private async Task<User> GetRequesterAsync(int requesterId) =>
        await _context.Users.FindAsync(requesterId)
        ?? throw new UnauthorizedAccessException("Usuario invalido.");
}
