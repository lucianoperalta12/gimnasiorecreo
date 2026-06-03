using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class EgresoConfiguration : IEntityTypeConfiguration<Egreso>
{
    public void Configure(EntityTypeBuilder<Egreso> builder)
    {
        builder.ToTable("Egresos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(200);

        builder.Property(e => e.Categoria)
            .HasMaxLength(100);

        builder.Property(e => e.Monto)
            .HasPrecision(10, 2);

        builder.Property(e => e.Fecha)
            .HasColumnType("timestamp without time zone");

        builder.Property(e => e.Observaciones)
            .HasMaxLength(1000);

        builder.Property(e => e.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(e => e.Gym)
            .WithMany(g => g.Egresos)
            .HasForeignKey(e => e.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.GymId, e.Fecha });
    }
}
