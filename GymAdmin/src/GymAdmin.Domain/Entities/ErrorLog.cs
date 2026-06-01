namespace GymAdmin.Domain.Entities;

public class ErrorLog
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
