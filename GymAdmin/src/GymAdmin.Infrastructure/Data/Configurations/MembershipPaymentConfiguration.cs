using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class MembershipPaymentConfiguration : IEntityTypeConfiguration<MembershipPayment>
{
    public void Configure(EntityTypeBuilder<MembershipPayment> builder)
    {
        builder.ToTable("MembershipPayments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Monto)
            .HasPrecision(10, 2);

        builder.Property(p => p.MetodoPago)
            .HasMaxLength(50);

        builder.Property(p => p.Estado)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Referencia)
            .HasMaxLength(200);

        builder.Property(p => p.Notas)
            .HasMaxLength(1000);

        builder.Property(p => p.FechaPago)
            .HasColumnType("timestamp without time zone");

        builder.Property(p => p.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(p => p.Gym)
            .WithMany(g => g.MembershipPayments)
            .HasForeignKey(p => p.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Membresia)
            .WithMany(m => m.Pagos)
            .HasForeignKey(p => p.MembresiaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.GymId, p.MembresiaId });
        builder.HasIndex(p => p.FechaPago);
    }
}
