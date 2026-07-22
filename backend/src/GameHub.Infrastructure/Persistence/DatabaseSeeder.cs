using GameHub.Application.Abstractions.Security;
using GameHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameHub.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    // Dev convenience: ensure a default admin exists so a fresh database (e.g. the
    // container's) is usable immediately. Gated by config (SeedAdmin) — never for prod.
    // Idempotent: does nothing if the admin already exists.
    public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["Seed:AdminEmail"] ?? "admin@gamehub.local";
        var username = configuration["Seed:AdminUsername"] ?? "admin";
        var password = configuration["Seed:AdminPassword"] ?? "Admin123!";

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameHubDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (await dbContext.Users.AnyAsync(user => user.Email == email))
            return;

        var admin = new User(username, email, passwordHasher.Hash(password));
        admin.PromoteToAdmin();

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();
    }
}
