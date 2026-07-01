using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoCorreo)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DestinatarioNombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.DestinatarioApellido)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.DestinatarioDni)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DestinatarioEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FechaEnvio)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Gym)
            .WithMany()
            .HasForeignKey(x => x.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TipoCorreo);
        builder.HasIndex(x => x.FechaEnvio);
        builder.HasIndex(x => x.DestinatarioDni);
    }
}
