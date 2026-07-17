using GameHub.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameHub.Infrastructure.Persistence.Configurations;

public sealed class PlatformConfiguration : IEntityTypeConfiguration<Platform>
{
    public void Configure(EntityTypeBuilder<Platform> builder)
    {
        // Table
        builder.ToTable("Platforms");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships

        // Indexes
        builder.HasIndex(p => p.Name)
            .IsUnique();
    }
}