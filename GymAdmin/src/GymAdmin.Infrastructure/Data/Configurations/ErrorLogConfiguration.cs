using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAdmin.Infrastructure.Data.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("ErrorLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired();

        builder.Property(x => x.Path)
            .HasMaxLength(2048);

        builder.Property(x => x.Method)
            .HasMaxLength(50);

        builder.Property(x => x.Timestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.Timestamp);
    }
}
