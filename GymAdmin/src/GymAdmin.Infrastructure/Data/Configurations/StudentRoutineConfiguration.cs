using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class StudentRoutineConfiguration : IEntityTypeConfiguration<StudentRoutine>
{
    public void Configure(EntityTypeBuilder<StudentRoutine> builder)
    {
        builder.ToTable("StudentRoutines");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.FechaAsignacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sr => sr.Activa)
            .HasDefaultValue(true);

        builder.HasOne(sr => sr.Alumno)
            .WithMany(u => u.RutinasAsignadas)
            .HasForeignKey(sr => sr.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.Rutina)
            .WithMany(r => r.AlumnosAsignados)
            .HasForeignKey(sr => sr.RutinaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevent duplicate assignment of same routine to same student
        builder.HasIndex(sr => new { sr.AlumnoId, sr.RutinaId })
            .IsUnique();
    }
}
