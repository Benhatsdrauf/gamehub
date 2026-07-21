using System.Text;
using GameHub.Application;
using GameHub.Infrastructure;
using GameHub.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Read the same "Jwt" section the token generator uses. The API needs the secret
// and issuer/audience to VALIDATE incoming tokens — the mirror image of signing.
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are missing from configuration.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep the claim names exactly as we minted them (sub/email/role) instead of
        // letting the handler rewrite short names into long legacy URIs.
        options.MapInboundClaims = false;

        // Every check the middleware runs on an incoming token before trusting it.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            // The claim [Authorize(Roles = "...")] reads. We minted role as "role".
            RoleClaimType = "role",
            // Tolerance for clock differences between servers; default is 5 minutes.
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Interactive API testing UI (Development only), served at /scalar.
    app.MapScalarApiReference();
}

// Order matters: authentication reads the token and sets HttpContext.User;
// authorization then decides if that user may proceed. Identify, then permit.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
