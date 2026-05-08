using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class RoutineExerciseConfiguration : IEntityTypeConfiguration<RoutineExercise>
{
    public void Configure(EntityTypeBuilder<RoutineExercise> builder)
    {
        builder.ToTable("RoutineExercises");

        builder.HasKey(re => re.Id);

        builder.Property(re => re.Bloque)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(re => re.Series)
            .IsRequired();

        builder.Property(re => re.Repeticiones)
            .IsRequired();

        builder.Property(re => re.Peso)
            .HasPrecision(8, 2);

        builder.Property(re => re.Observaciones)
            .HasMaxLength(500);

        builder.HasOne(re => re.Rutina)
            .WithMany(r => r.Ejercicios)
            .HasForeignKey(re => re.RutinaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(re => re.Ejercicio)
            .WithMany(e => e.RutinaEjercicios)
            .HasForeignKey(re => re.EjercicioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(re => new { re.RutinaId, re.Orden })
            .IsUnique();
    }
}
