using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class RoutineConfiguration : IEntityTypeConfiguration<Routine>
{
    public void Configure(EntityTypeBuilder<Routine> builder)
    {
        builder.ToTable("Routines");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(1000);

        builder.Property(r => r.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.Activa)
            .HasDefaultValue(true);

        builder.HasOne(r => r.Profesor)
            .WithMany(u => u.RutinasCreadas)
            .HasForeignKey(r => r.ProfesorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
