using GameHub.Domain.Authentication;
using GameHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameHub.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(128); // SHA-256 hex is 64 chars; headroom for future algorithms

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.ExpiresAtUtc).IsRequired();
        // RevokedAtUtc and ReplacedByTokenId are nullable — mapped as nullable by default.

        // Every /refresh looks a token up by its hash, so index it; it is also unique.
        builder.HasIndex(t => t.TokenHash).IsUnique();

        // Reuse-detection revokes all of a user's tokens — index the lookup.
        builder.HasIndex(t => t.UserId);

        // Owned by a user; deleting the user deletes their tokens.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
