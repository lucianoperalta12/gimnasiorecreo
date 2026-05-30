using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Egresos;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class EgresoService : IEgresoService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EgresoService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResult<EgresoListDto>> GetAllAsync(int requesterId, int? gymId = null, int? page = null, int? pageSize = null)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManage(requester);

        var query = _context.Egresos.AsNoTracking().AsQueryable();
        query = ApplyGymFilter(query, requester, gymId);

        var totalCount = await query.CountAsync();
        var pagedQuery = ApplyPagination(query.OrderByDescending(e => e.Fecha), page, pageSize);

        var items = await pagedQuery
            .Select(e => new EgresoListDto(e.Id, e.GymId, e.Descripcion, e.Categoria, e.Monto, e.Fecha, e.Observaciones))
            .ToListAsync();

        return new PagedResult<EgresoListDto>(items, totalCount, page, NormalizePageSize(pageSize));
    }

    public async Task<EgresoDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManage(requester);

        var egreso = await _context.Egresos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null) return null;

        EnsureSameGym(requester, egreso.GymId);
        return MapToDto(egreso);
    }

    public async Task<EgresoDto> CreateAsync(int requesterId, CreateEgresoRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManage(requester);

        if (string.IsNullOrWhiteSpace(request.Descripcion))
            throw new ArgumentException("La descripción es obligatoria.");
        if (string.IsNullOrWhiteSpace(request.Categoria))
            throw new ArgumentException("La categoría es obligatoria.");
        if (request.Monto <= 0)
            throw new ArgumentException("El monto debe ser mayor a cero.");

        var egreso = new Egreso
        {
            GymId = requester.GymId,
            Descripcion = request.Descripcion.Trim(),
            Categoria = request.Categoria.Trim(),
            Monto = request.Monto,
            Fecha = request.Fecha.ToUniversalTime(),
            Observaciones = request.Observaciones?.Trim()
        };

        _context.Egresos.Add(egreso);
        await _context.SaveChangesAsync();

        return MapToDto(egreso);
    }

    public async Task<EgresoDto> UpdateAsync(int requesterId, int id, UpdateEgresoRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManage(requester);

        if (string.IsNullOrWhiteSpace(request.Descripcion))
            throw new ArgumentException("La descripción es obligatoria.");
        if (string.IsNullOrWhiteSpace(request.Categoria))
            throw new ArgumentException("La categoría es obligatoria.");
        if (request.Monto <= 0)
            throw new ArgumentException("El monto debe ser mayor a cero.");

        var egreso = await _context.Egresos.FindAsync(id)
            ?? throw new KeyNotFoundException("Egreso no encontrado.");

        EnsureSameGym(requester, egreso.GymId);

        egreso.Descripcion = request.Descripcion.Trim();
        egreso.Categoria = request.Categoria.Trim();
        egreso.Monto = request.Monto;
        egreso.Fecha = request.Fecha.ToUniversalTime();
        egreso.Observaciones = request.Observaciones?.Trim();

        await _context.SaveChangesAsync();
        return MapToDto(egreso);
    }

    public async Task DeleteAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManage(requester);

        var egreso = await _context.Egresos.FindAsync(id)
            ?? throw new KeyNotFoundException("Egreso no encontrado.");

        EnsureSameGym(requester, egreso.GymId);

        _context.Egresos.Remove(egreso);
        await _context.SaveChangesAsync();
    }

    private static EgresoDto MapToDto(Egreso e) =>
        new(e.Id, e.GymId, e.Descripcion, e.Categoria, e.Monto, e.Fecha, e.Observaciones, e.FechaCreacion);

    private static IQueryable<Egreso> ApplyGymFilter(IQueryable<Egreso> query, User requester, int? gymId)
    {
        if (requester.Rol == UserRole.Superusuario)
        {
            if (gymId.HasValue && gymId.Value > 0)
                query = query.Where(e => e.GymId == gymId.Value);
            return query;
        }
        return query.Where(e => e.GymId == requester.GymId);
    }

    private static void EnsureCanManage(User requester)
    {
        if (requester.Rol is not (UserRole.Superusuario or UserRole.Administrativo))
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

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var gymIdClaim = httpContext.User.FindFirst("gymId")?.Value;
            if (int.TryParse(gymIdClaim, out var gymId))
                user.GymId = gymId;

            var roleClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (Enum.TryParse<UserRole>(roleClaim, true, out var role))
                user.Rol = role;
        }

        return user;
    }

    private static IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int? page, int? pageSize)
    {
        var normalizedPageSize = NormalizePageSize(pageSize);
        if (!normalizedPageSize.HasValue) return query;
        var normalizedPage = page.GetValueOrDefault(1) > 0 ? page.GetValueOrDefault(1) : 1;
        return query.Skip((normalizedPage - 1) * normalizedPageSize.Value).Take(normalizedPageSize.Value);
    }

    private static int? NormalizePageSize(int? pageSize)
    {
        if (!pageSize.HasValue || pageSize.Value <= 0) return null;
        return Math.Min(pageSize.Value, 200);
    }
}
