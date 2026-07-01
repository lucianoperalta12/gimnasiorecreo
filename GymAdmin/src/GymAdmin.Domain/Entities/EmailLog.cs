using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Entities;

public class EmailLog
{
    public int Id { get; set; }
    public TipoCorreo TipoCorreo { get; set; }
    public string DestinatarioNombre { get; set; } = string.Empty;
    public string DestinatarioApellido { get; set; } = string.Empty;
    public string DestinatarioDni { get; set; } = string.Empty;
    public string DestinatarioEmail { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; } = DateTime.Now;
    public int? GymId { get; set; }
    public bool Exitoso { get; set; }
    public string? ErrorMensaje { get; set; }

    // Navigation property (optional, if Gym entity is required)
    public Gym? Gym { get; set; }
}
