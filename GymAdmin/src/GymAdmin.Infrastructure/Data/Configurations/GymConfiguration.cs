using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder.ToTable("Gyms");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(g => g.DuenoNombreApellido).IsRequired().HasMaxLength(200);
        builder.Property(g => g.LogoUrl).HasMaxLength(500);
        builder.Property(g => g.ColorPrincipalHex).IsRequired().HasMaxLength(7);
        builder.Property(g => g.Activo).HasDefaultValue(true);
        builder.Property(g => g.VeRutinas).HasDefaultValue(true);
        builder.Property(g => g.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
