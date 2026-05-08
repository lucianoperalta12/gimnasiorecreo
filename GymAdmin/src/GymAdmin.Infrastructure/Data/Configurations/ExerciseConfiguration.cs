using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(1000);

        builder.Property(e => e.GrupoMuscular)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.VideoUrl)
            .HasMaxLength(500);
    }
}
