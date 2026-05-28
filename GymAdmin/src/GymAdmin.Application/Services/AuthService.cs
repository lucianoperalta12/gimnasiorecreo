using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using GymAdmin.Application.DTOs.Auth;
using GymAdmin.Application.DTOs.Users;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GymAdmin.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public AuthService(AppDbContext context, IConfiguration configuration, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponse> LoginWithGoogleAsync(string credential)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [_configuration["Google:ClientId"]] };
        var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Email == payload.Email)
            ?? throw new UnauthorizedAccessException("Usuario no habilitado. Contacte al administrador.");

        if (!user.Activo) throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");
        if (string.IsNullOrEmpty(user.GoogleId))
        {
            user.GoogleId = payload.Subject;
            await _context.SaveChangesAsync();
        }

        return await ProcessGymSelectionOrLoginAsync(user);
    }

    public async Task<AuthResponse> LoginAdminAsync(string username, string password)
    {
        var lowerUsername = username.Trim().ToLowerInvariant();
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == lowerUsername ||
                u.Dni.ToLower() == lowerUsername ||
                (u.Nombre.ToLower() + " " + u.Apellido.ToLower()) == lowerUsername);

        if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!user.Activo) throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");

        return await ProcessGymSelectionOrLoginAsync(user);
    }

    public Task<AuthResponse> RegisterAsync(string nombre, string email, string password)
        => throw new InvalidOperationException("El registro público está deshabilitado. Los usuarios se crean desde el panel.");

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiracion < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        if (!user.Activo) throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");

        var gymId = GetGymIdFromHeader() ?? user.GymUsers.FirstOrDefault(gu => gu.Activo && gu.Gym.Activo)?.GymId;
        if (gymId is null)
            throw new UnauthorizedAccessException("Asociación de gimnasio no encontrada o inactiva.");

        var assoc = user.GymUsers.FirstOrDefault(gu => gu.GymId == gymId.Value && gu.Activo && gu.Gym.Activo);
        if (assoc == null)
            throw new UnauthorizedAccessException("Asociación de gimnasio no encontrada o inactiva.");

        return await BuildAuthResponseAsync(user, assoc);
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;
        user.RefreshToken = null;
        user.RefreshTokenExpiracion = null;
        await _context.SaveChangesAsync();
    }

    public async Task<AuthResponse> SelectGymAsync(int userId, int gymId)
    {
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

        if (!user.Activo) throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");

        var assoc = user.GymUsers.FirstOrDefault(gu => gu.GymId == gymId && gu.Activo && gu.Gym.Activo);
        if (assoc == null)
            throw new UnauthorizedAccessException("No perteneces a este gimnasio o tu cuenta está inactiva en él.");

        return await BuildAuthResponseAsync(user, assoc);
    }

    private async Task<AuthResponse> ProcessGymSelectionOrLoginAsync(User user)
    {
        var activeAssociations = user.GymUsers.Where(gu => gu.Activo && gu.Gym.Activo).ToList();
        if (!activeAssociations.Any())
            throw new UnauthorizedAccessException("El usuario no tiene asociaciones activas a ningún gimnasio.");

        if (activeAssociations.Count == 1)
        {
            return await BuildAuthResponseAsync(user, activeAssociations[0]);
        }

        var tempToken = GenerateAccessToken(user, null, null);
        var gyms = BuildGymAssociationDtos(activeAssociations);

        return new AuthResponse(
            tempToken,
            null,
            null,
            true,
            gyms
        );
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, GymUser association)
    {
        var activeAssociations = user.GymUsers.Where(gu => gu.Activo && gu.Gym.Activo).ToList();
        var gyms = BuildGymAssociationDtos(activeAssociations);

        var accessToken = GenerateAccessToken(user, association.GymId, association.Rol);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiracion = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponse(
            accessToken,
            refreshToken,
            new UserDto(
                user.Id,
                user.Nombre,
                user.Apellido,
                user.Email,
                user.Dni,
                association.Rol.ToString(),
                user.Activo && association.Activo,
                user.DebeCambiarPassword,
                association.GymId,
                association.Gym.Nombre,
                association.Gym.ColorPrincipalHex,
                association.Gym.LogoUrl,
                association.Gym.VeRutinas,
                user.FechaCreacion,
                user.FechaNacimiento,
                user.Domicilio,
                user.Telefono,
                user.Observaciones
            ),
            false,
            gyms.Count > 1 ? gyms : null
        );
    }

    private static List<GymAssociationDto> BuildGymAssociationDtos(IEnumerable<GymUser> activeAssociations) =>
        activeAssociations
            .Select(gu => new GymAssociationDto(
                gu.GymId,
                gu.Gym.Nombre,
                gu.Gym.LogoUrl,
                gu.Gym.ColorPrincipalHex,
                gu.Rol.ToString()
            ))
            .ToList();

    private string GenerateAccessToken(User user, int? gymId, UserRole? role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.Nombre} {user.Apellido}".Trim())
        };

        if (role.HasValue)
            claims.Add(new Claim(ClaimTypes.Role, role.Value.ToString()));

        if (gymId.HasValue)
            claims.Add(new Claim("gymId", gymId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int? GetGymIdFromHeader()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return null;

        var tokenStr = authHeader.Substring("Bearer ".Length).Trim();
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(tokenStr) as JwtSecurityToken;
            var gymIdClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == "gymId")?.Value;
            if (int.TryParse(gymIdClaim, out var gymId)) return gymId;
        }
        catch
        {
        }

        return null;
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
