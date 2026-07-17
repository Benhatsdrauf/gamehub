using GameHub.Domain.Core;
using GameHub.Domain.Games;

namespace GameHub.Domain.Users;

public sealed class DeveloperProfile : BaseEntity
{
    private readonly List<Game> _games = [];

    public string DisplayName { get; private set; }

    public string? Bio { get; private set; }

    public string? Website { get; private set; }

    public string? LogoPath { get; private set; }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public IReadOnlyCollection<Game> Games => _games;

    protected DeveloperProfile()
    {
        DisplayName = string.Empty;
    }

    public DeveloperProfile(
        User user,
        string displayName)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
        UserId = user.Id;

        SetDisplayName(displayName);

        user.CreateDeveloperProfile(this);
    }

    public void Rename(string displayName)
    {
        SetDisplayName(displayName);
    }

    public void UpdateBio(string? bio)
    {
        Bio = string.IsNullOrWhiteSpace(bio)
            ? null
            : bio.Trim();
    }

    public void UpdateWebsite(string? website)
    {
        Website = string.IsNullOrWhiteSpace(website)
            ? null
            : website.Trim();
    }

    public void UpdateLogo(string? logoPath)
    {
        LogoPath = string.IsNullOrWhiteSpace(logoPath)
            ? null
            : logoPath.Trim();
    }

    private void SetDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        DisplayName = displayName.Trim();
    }
}