using GymAdmin.Application.DTOs.MembershipPlans;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class MembershipPlanService : IMembershipPlanService
{
    private readonly AppDbContext _context;

    public MembershipPlanService(AppDbContext context) => _context = context;

    public async Task<List<MembershipPlanDto>> GetAllAsync(int requesterId, int? gymId = null)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePlans(requester);

        var query = _context.MembershipPlans.AsNoTracking().Include(p => p.Gym).AsQueryable();
        query = ApplyGymFilter(query, requester, gymId);

        return await query
            .OrderBy(p => p.Nombre)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<MembershipPlanDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePlans(requester);

        var query = _context.MembershipPlans.AsNoTracking().Include(p => p.Gym).Where(p => p.Id == id);
        query = ApplyGymFilter(query, requester, null);

        var plan = await query.FirstOrDefaultAsync();
        return plan is null ? null : MapToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(int requesterId, CreateMembershipPlanRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePlans(requester);

        if (request.DuracionDias <= 0) throw new ArgumentException("La duración debe ser mayor a cero.");
        if (request.Precio < 0) throw new ArgumentException("El precio no puede ser negativo.");

        var gymId = requester.Rol == UserRole.Superusuario ? request.GymId ?? 0 : requester.GymId;
        if (gymId <= 0) throw new ArgumentException("Debe indicar gimnasio.");

        if (!await _context.Gyms.AnyAsync(g => g.Id == gymId && g.Activo))
            throw new KeyNotFoundException("Gimnasio no encontrado.");

        var plan = new MembershipPlan
        {
            GymId = gymId,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            DuracionDias = request.DuracionDias,
            Precio = request.Precio,
            Activo = request.Activo
        };

        _context.MembershipPlans.Add(plan);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(requesterId, plan.Id))!;
    }

    public async Task<MembershipPlanDto> UpdateAsync(int requesterId, int id, UpdateMembershipPlanRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePlans(requester);

        if (request.DuracionDias <= 0) throw new ArgumentException("La duración debe ser mayor a cero.");
        if (request.Precio < 0) throw new ArgumentException("El precio no puede ser negativo.");

        var plan = await _context.MembershipPlans.FindAsync(id)
            ?? throw new KeyNotFoundException("Plan no encontrado.");

        EnsureSameGym(requester, plan.GymId);

        plan.Nombre = request.Nombre.Trim();
        plan.Descripcion = request.Descripcion?.Trim();
        plan.DuracionDias = request.DuracionDias;
        plan.Precio = request.Precio;
        plan.Activo = request.Activo;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(requesterId, plan.Id))!;
    }

    public async Task DeleteAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePlans(requester);

        var plan = await _context.MembershipPlans.FindAsync(id)
            ?? throw new KeyNotFoundException("Plan no encontrado.");

        EnsureSameGym(requester, plan.GymId);

        if (await _context.Memberships.AnyAsync(m => m.PlanId == id))
            throw new InvalidOperationException("No se puede eliminar un plan con membresías asociadas. Desactívelo en su lugar.");

        _context.MembershipPlans.Remove(plan);
        await _context.SaveChangesAsync();
    }

    private static MembershipPlanDto MapToDto(MembershipPlan p) =>
        new(p.Id, p.GymId, p.Gym?.Nombre, p.Nombre, p.Descripcion, p.DuracionDias, p.Precio, p.Activo, p.FechaCreacion, p.Gym?.Moneda);

    private static IQueryable<MembershipPlan> ApplyGymFilter(
        IQueryable<MembershipPlan> query,
        User requester,
        int? gymId)
    {
        if (requester.Rol == UserRole.Superusuario)
        {
            if (gymId.HasValue && gymId.Value > 0)
                query = query.Where(p => p.GymId == gymId.Value);
            return query;
        }

        return query.Where(p => p.GymId == requester.GymId);
    }

    private static void EnsureCanManagePlans(User requester)
    {
        if (requester.Rol is not (UserRole.Superusuario or UserRole.Administrativo))
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
