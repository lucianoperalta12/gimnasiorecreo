using System;
using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Entities;

public class GymUser
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public int UserId { get; set; }
    public UserRole Rol { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAsociacion { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Gym Gym { get; set; } = null!;
    public User User { get; set; } = null!;
}
