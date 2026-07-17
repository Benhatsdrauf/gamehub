using GameHub.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameHub.Infrastructure.Persistence.Configurations;

public sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        // Table
        builder.ToTable("Genres");

        // Primary Key
        builder.HasKey(g => g.Id);

        // Properties
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships

        // Indexes
        builder.HasIndex(g => g.Name)
            .IsUnique();
    }
}