using GameHub.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameHub.Infrastructure.Persistence.Configurations;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        // Table
        builder.ToTable("Games");

        // Primary Key
        builder.HasKey(g => g.Id);

        // Properties
        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.ShortDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.Description)
            .IsRequired();

        builder.Property(g => g.Price)
            .HasPrecision(10, 2);

        builder.Property(g => g.CoverImagePath)
            .HasMaxLength(500);

        builder.Property(g => g.Status)
            .IsRequired();

        builder.Property(g => g.ReleaseDate);

        builder.Property(g => g.PublishedAt);

        // Relationships
        builder.HasOne(g => g.DeveloperProfile)
            .WithMany(d => d.Games)
            .HasForeignKey(g => g.DeveloperProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Genres)
            .WithMany();

        builder.HasMany(g => g.Platforms)
            .WithMany();

        // Indexes
        builder.HasIndex(g => g.Title);
    }
}