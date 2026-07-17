using GameHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameHub.Infrastructure.Persistence.Configurations;

public sealed class DeveloperProfileConfiguration : IEntityTypeConfiguration<DeveloperProfile>
{
    public void Configure(EntityTypeBuilder<DeveloperProfile> builder)
    {
        // Table
        builder.ToTable("DeveloperProfiles");

        // Primary Key
        builder.HasKey(d => d.Id);

        // Properties
        builder.Property(d => d.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Bio)
            .HasMaxLength(2000);

        builder.Property(d => d.Website)
            .HasMaxLength(255);

        builder.Property(d => d.LogoPath)
            .HasMaxLength(500);

        // Relationships

        // Indexes
        builder.HasIndex(d => d.DisplayName)
            .IsUnique();
    }
}