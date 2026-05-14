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

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginWithGoogleAsync(string credential)
    {
        GoogleJsonWebSignature.Payload payload;
        var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [_configuration["Google:ClientId"]] };
        payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

        var user = await _context.Users.Include(u => u.Gym).FirstOrDefaultAsync(u => u.Email == payload.Email)
            ?? throw new UnauthorizedAccessException("Usuario no habilitado. Contacte al administrador.");

        if (!user.Activo) throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");
        if (string.IsNullOrEmpty(user.GoogleId)) user.GoogleId = payload.Subject;
        await _context.SaveChangesAsync();
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAdminAsync(string username, string password)
    {
        var lowerUsername = username.Trim().ToLowerInvariant();
        var user = await _context.Users.Include(u => u.Gym).FirstOrDefaultAsync(u =>
            u.Email.ToLower() == lowerUsername ||
            u.Dni.ToLower() == lowerUsername ||
            (u.Nombre.ToLower() + " " + u.Apellido.ToLower()) == lowerUsername);

        if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        if (!user.Activo) throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");
        return GenerateAuthResponse(user);
    }

    public Task<AuthResponse> RegisterAsync(string nombre, string email, string password)
        => throw new InvalidOperationException("El registro público está deshabilitado. Los usuarios se crean desde el panel.");

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users.Include(u => u.Gym).FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null || user.RefreshTokenExpiracion < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
        if (!user.Activo) throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");
        return GenerateAuthResponse(user);
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;
        user.RefreshToken = null;
        user.RefreshTokenExpiracion = null;
        await _context.SaveChangesAsync();
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiracion = DateTime.UtcNow.AddDays(7);
        _context.SaveChanges();

        return new AuthResponse(
            accessToken,
            refreshToken,
            new UserDto(user.Id, user.Nombre, user.Apellido, user.Email, user.Dni, user.Rol.ToString(), user.Activo, user.DebeCambiarPassword, user.GymId, user.Gym?.Nombre, user.Gym?.ColorPrincipalHex, user.Gym?.LogoUrl, user.FechaCreacion)
        );
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.Nombre} {user.Apellido}".Trim()),
            new Claim(ClaimTypes.Role, user.Rol.ToString()),
            new Claim("gymId", user.GymId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
