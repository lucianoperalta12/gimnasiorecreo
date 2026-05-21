using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Apellido)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Dni)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(u => u.Email)
            .IsUnique();
        builder.HasIndex(u => u.Dni)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired(false);

        builder.Property(u => u.GoogleId)
            .HasMaxLength(200);

        builder.Property(u => u.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserRole.Alumno);

        builder.Property(u => u.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.Domicilio)
            .HasMaxLength(255);
            
        builder.Property(u => u.Telefono)
            .HasMaxLength(50);
            
        builder.Property(u => u.Observaciones)
            .HasMaxLength(1000);

        builder.HasOne(u => u.Gym)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GymId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
