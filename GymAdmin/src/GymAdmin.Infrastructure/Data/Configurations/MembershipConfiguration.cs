using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(MembershipStatus.Activa);

        builder.Property(m => m.Notas)
            .HasMaxLength(1000);

        builder.Property(m => m.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(m => m.Gym)
            .WithMany(g => g.Memberships)
            .HasForeignKey(m => m.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Alumno)
            .WithMany(u => u.Membresias)
            .HasForeignKey(m => m.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Plan)
            .WithMany(p => p.Membresias)
            .HasForeignKey(m => m.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.GymId, m.AlumnoId });
        builder.HasIndex(m => m.FechaVencimiento);
        builder.HasIndex(m => m.Estado);

        // Un alumno solo puede tener una membresía activa a la vez
        builder.HasIndex(m => m.AlumnoId)
            .IsUnique()
            .HasFilter($"\"Estado\" = '{MembershipStatus.Activa}'");
    }
}
