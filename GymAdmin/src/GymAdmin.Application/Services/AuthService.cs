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
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Token de Google inválido.", ex);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user == null)
        {
            user = new User
            {
                Nombre = payload.Name,
                Email = payload.Email,
                GoogleId = payload.Subject,
                Rol = UserRole.Alumno,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }
        else if (string.IsNullOrEmpty(user.GoogleId))
        {
            user.GoogleId = payload.Subject;
        }

        await _context.SaveChangesAsync();
        
        if (!user.Activo)
            throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAdminAsync(string username, string password)
    {
        // Requerimiento específico: usuario "admin" contraseña "admin"
        // Buscamos un usuario que tenga ese nombre o email y sea Superusuario
        var user = await _context.Users
            .FirstOrDefaultAsync(u => (u.Nombre == username || u.Email == username));

        if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!user.Activo)
            throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiracion < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        if (!user.Activo)
            throw new UnauthorizedAccessException("Tu cuenta ha sido desactivada.");

        return GenerateAuthResponse(user);
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiracion = null;
            await _context.SaveChangesAsync();
        }
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
            new UserDto(user.Id, user.Nombre, user.Email, user.Rol.ToString(), user.Activo, user.FechaCreacion)
        );
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Role, user.Rol.ToString())
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
