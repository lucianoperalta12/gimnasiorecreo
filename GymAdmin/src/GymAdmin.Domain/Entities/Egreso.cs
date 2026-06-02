namespace GymAdmin.Domain.Entities;

public class Egreso
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public Gym Gym { get; set; } = null!;
}
