using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ToTable("MembershipPlans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(1000);

        builder.Property(p => p.Precio)
            .HasPrecision(10, 2);

        builder.Property(p => p.Activo)
            .HasDefaultValue(true);

        builder.Property(p => p.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(p => p.Gym)
            .WithMany(g => g.MembershipPlans)
            .HasForeignKey(p => p.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.GymId, p.Nombre });
    }
}
