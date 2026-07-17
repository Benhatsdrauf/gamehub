using GameHub.Domain.Games;
using GameHub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Infrastructure.Persistence;

public sealed class GameHubDbContext : DbContext
{
    public GameHubDbContext(DbContextOptions<GameHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<DeveloperProfile> DeveloperProfiles => Set<DeveloperProfile>();

    public DbSet<Game> Games => Set<Game>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Platform> Platforms => Set<Platform>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(GameHubDbContext).Assembly);
    }
}