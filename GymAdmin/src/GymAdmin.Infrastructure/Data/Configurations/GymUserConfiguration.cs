using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class GymUserConfiguration : IEntityTypeConfiguration<GymUser>
{
    public void Configure(EntityTypeBuilder<GymUser> builder)
    {
        builder.ToTable("GymUsers");

        builder.HasKey(gu => gu.Id);

        builder.Property(gu => gu.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(gu => gu.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(gu => gu.FechaAsociacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique index to prevent duplicate associations
        builder.HasIndex(gu => new { gu.GymId, gu.UserId })
            .IsUnique();

        // Foreign keys configuration
        builder.HasOne(gu => gu.Gym)
            .WithMany(g => g.GymUsers)
            .HasForeignKey(gu => gu.GymId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gu => gu.User)
            .WithMany(u => u.GymUsers)
            .HasForeignKey(gu => gu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
