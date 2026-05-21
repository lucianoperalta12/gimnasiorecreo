using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class IngresoConfiguration : IEntityTypeConfiguration<Ingreso>
{
    public void Configure(EntityTypeBuilder<Ingreso> builder)
    {
        builder.ToTable("Ingresos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FechaHora)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Gym)
            .WithMany(g => g.Ingresos)
            .HasForeignKey(x => x.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Alumno)
            .WithMany(u => u.IngresosComoAlumno)
            .HasForeignKey(x => x.AlumnoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Terminal)
            .WithMany(u => u.IngresosRegistrados)
            .HasForeignKey(x => x.TerminalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Membership)
            .WithMany(m => m.Ingresos)
            .HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FechaHora);
        builder.HasIndex(x => new { x.GymId, x.AlumnoId });
    }
}
