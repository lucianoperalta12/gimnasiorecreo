namespace GymAdmin.Domain.Entities;

public class MembershipPlan
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DuracionDias { get; set; }
    public decimal Precio { get; set; }
    public bool PaseLibre { get; set; } = true;
    public int? DiasPorSemana { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public Gym Gym { get; set; } = null!;
    public ICollection<Membership> Membresias { get; set; } = new List<Membership>();
}
