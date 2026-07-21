using GameHub.Application.Abstractions.Security;
using GameHub.Application.Users;
using GameHub.Infrastructure.Persistence;
using GameHub.Infrastructure.Persistence.Queries;
using GameHub.Infrastructure.Persistence.Repositories;
using GameHub.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GameHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<GameHubDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        // Scoped: both depend on the DbContext, which is itself scoped to one request.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserQueries, UserQueries>();

        // Singleton: stateless and thread-safe, so one instance can serve every request.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Bind the "Jwt" config section to JwtSettings, injectable as IOptions<JwtSettings>.
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Singleton: also stateless — it only reads settings and signs strings.
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}