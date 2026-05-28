using GymAdmin.Application.DTOs.Payments;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public PaymentService(AppDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<PaymentListDto>> GetAllAsync(int requesterId, int? gymId = null, int? membresiaId = null)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePayments(requester);

        var query = _context.MembershipPayments
            .AsNoTracking()
            .Include(p => p.Membresia)
                .ThenInclude(m => m.Alumno)
            .AsQueryable();

        query = ApplyGymFilter(query, requester, gymId);

        if (membresiaId.HasValue)
            query = query.Where(p => p.MembresiaId == membresiaId.Value);

        return await query
            .OrderByDescending(p => p.FechaPago)
            .Select(p => new PaymentListDto(
                p.Id,
                p.MembresiaId,
                (p.Membresia.Alumno.Nombre + " " + p.Membresia.Alumno.Apellido).Trim(),
                p.Monto,
                p.FechaPago,
                p.Estado.ToString(),
                p.MetodoPago))
            .ToListAsync();
    }

    public async Task<PaymentDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePayments(requester);

        var payment = await LoadPaymentAsync(id);
        if (payment is null) return null;

        EnsureSameGym(requester, payment.GymId);
        return MapToDto(payment);
    }

    public async Task<List<PaymentListDto>> GetByMembershipIdAsync(int requesterId, int membresiaId)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePayments(requester);

        var membership = await _context.Memberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membresiaId)
            ?? throw new KeyNotFoundException("Membresía no encontrada.");

        EnsureSameGym(requester, membership.GymId);

        return await GetAllAsync(requesterId, membership.GymId, membresiaId);
    }

    public async Task<PaymentDto> CreateAsync(int requesterId, CreatePaymentRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePayments(requester);

        if (request.Monto <= 0) throw new ArgumentException("El monto debe ser mayor a cero.");
        if (!Enum.TryParse<PaymentStatus>(request.Estado, true, out var estado))
            throw new ArgumentException("Estado de pago inválido.");

        var membership = await _context.Memberships
            .Include(m => m.Alumno)
            .FirstOrDefaultAsync(m => m.Id == request.MembresiaId)
            ?? throw new KeyNotFoundException("Membresía no encontrada.");

        EnsureSameGym(requester, membership.GymId);

        var payment = new MembershipPayment
        {
            GymId = membership.GymId,
            MembresiaId = membership.Id,
            Monto = request.Monto,
            FechaPago = request.FechaPago.ToUniversalTime(),
            MetodoPago = request.MetodoPago?.Trim(),
            Estado = estado,
            Referencia = request.Referencia?.Trim(),
            Notas = request.Notas?.Trim()
        };

        _context.MembershipPayments.Add(payment);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(requesterId, payment.Id))!;
    }

    public async Task<PaymentDto> UpdateAsync(int requesterId, int id, UpdatePaymentRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePayments(requester);

        if (request.Monto <= 0) throw new ArgumentException("El monto debe ser mayor a cero.");
        if (!Enum.TryParse<PaymentStatus>(request.Estado, true, out var estado))
            throw new ArgumentException("Estado de pago inválido.");

        var payment = await _context.MembershipPayments
            .Include(p => p.Membresia)
                .ThenInclude(m => m.Alumno)
            .Include(p => p.Gym)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Pago no encontrado.");

        EnsureSameGym(requester, payment.GymId);

        payment.Monto = request.Monto;
        payment.FechaPago = request.FechaPago.ToUniversalTime();
        payment.MetodoPago = request.MetodoPago?.Trim();
        payment.Estado = estado;
        payment.Referencia = request.Referencia?.Trim();
        payment.Notas = request.Notas?.Trim();

        await _context.SaveChangesAsync();
        return MapToDto(payment);
    }

    public async Task DeleteAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        EnsureCanManagePayments(requester);

        var payment = await _context.MembershipPayments.FindAsync(id)
            ?? throw new KeyNotFoundException("Pago no encontrado.");

        EnsureSameGym(requester, payment.GymId);

        _context.MembershipPayments.Remove(payment);
        await _context.SaveChangesAsync();
    }

    private async Task<MembershipPayment?> LoadPaymentAsync(int id) =>
        await _context.MembershipPayments
            .AsNoTracking()
            .Include(p => p.Membresia)
                .ThenInclude(m => m.Alumno)
            .Include(p => p.Gym)
            .FirstOrDefaultAsync(p => p.Id == id);

    private static PaymentDto MapToDto(MembershipPayment p) =>
        new(
            p.Id,
            p.GymId,
            p.Gym?.Nombre,
            p.MembresiaId,
            p.Membresia.AlumnoId,
            (p.Membresia.Alumno.Nombre + " " + p.Membresia.Alumno.Apellido).Trim(),
            p.Monto,
            p.FechaPago,
            p.MetodoPago,
            p.Estado.ToString(),
            p.Referencia,
            p.Notas,
            p.FechaCreacion
        );

    private static IQueryable<MembershipPayment> ApplyGymFilter(
        IQueryable<MembershipPayment> query,
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

    private static void EnsureCanManagePayments(User requester)
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
            {
                user.GymId = gymId;
            }

            var roleClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (Enum.TryParse<UserRole>(roleClaim, true, out var role))
            {
                user.Rol = role;
            }
        }

        return user;
    }
}
