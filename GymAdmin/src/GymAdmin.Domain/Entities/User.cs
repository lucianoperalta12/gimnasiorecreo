using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public UserRole Rol { get; set; } = UserRole.Alumno;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiracion { get; set; }

    // Navigation properties
    public ICollection<Routine> RutinasCreadas { get; set; } = new List<Routine>();
    public ICollection<StudentRoutine> RutinasAsignadas { get; set; } = new List<StudentRoutine>();
}
