using GameHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameHub.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.AvatarPath)
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired();

        // Relationships
        builder.HasOne(u => u.DeveloperProfile)
            .WithOne(d => d.User)
            .HasForeignKey<DeveloperProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName(UserConstraintNames.UniqueUsername);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName(UserConstraintNames.UniqueEmail);
    }
}