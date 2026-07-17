using GameHub.Domain.Core;
using GameHub.Domain.Users;

namespace GameHub.Domain.Games;

public sealed class Game : BaseEntity
{
    private readonly List<Genre> _genres = [];
    private readonly List<Platform> _platforms = [];

    public string Title { get; private set; }

    public string ShortDescription { get; private set; }

    public string Description { get; private set; }

    public decimal Price { get; private set; }

    public DateOnly? ReleaseDate { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public string? CoverImagePath { get; private set; }

    public GameStatus Status { get; private set; }

    public Guid DeveloperProfileId { get; private set; }

    public DeveloperProfile DeveloperProfile { get; private set; } = null!;

    public IReadOnlyCollection<Genre> Genres => _genres;

    public IReadOnlyCollection<Platform> Platforms => _platforms;

    protected Game()
    {
        Title = string.Empty;
        ShortDescription = string.Empty;
        Description = string.Empty;
    }

    public Game(
        DeveloperProfile developerProfile,
        string title,
        string shortDescription,
        string description,
        decimal price)
    {
        ArgumentNullException.ThrowIfNull(developerProfile);

        DeveloperProfile = developerProfile;
        DeveloperProfileId = developerProfile.Id;

        SetTitle(title);
        SetShortDescription(shortDescription);
        SetDescription(description);
        SetPrice(price);

        Status = GameStatus.Draft;
    }

    public void Rename(string title)
    {
        SetTitle(title);
    }

    public void UpdateShortDescription(string shortDescription)
    {
        SetShortDescription(shortDescription);
    }

    public void UpdateDescription(string description)
    {
        SetDescription(description);
    }

    public void UpdatePrice(decimal price)
    {
        SetPrice(price);
    }

    public void SetReleaseDate(DateOnly? releaseDate)
    {
        ReleaseDate = releaseDate;
    }

    public void UpdateCoverImage(string? coverImagePath)
    {
        CoverImagePath = string.IsNullOrWhiteSpace(coverImagePath)
            ? null
            : coverImagePath.Trim();
    }

    public void Publish()
    {
        if (Status != GameStatus.PendingApproval)
            throw new InvalidOperationException("Only pending games can be published.");

        Status = GameStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void SubmitForApproval()
    {
        if (Status != GameStatus.Draft)
            throw new InvalidOperationException("Only draft games can be submitted.");

        Status = GameStatus.PendingApproval;
    }

    public void Reject()
    {
        if (Status != GameStatus.PendingApproval)
            throw new InvalidOperationException("Only pending games can be rejected.");

        Status = GameStatus.Rejected;
    }

    public void Archive()
    {
        if (Status != GameStatus.Published)
            throw new InvalidOperationException("Only published games can be archived.");

        Status = GameStatus.Archived;
    }

    public void AddGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        if (_genres.Contains(genre))
            return;

        _genres.Add(genre);
    }

    public void RemoveGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        _genres.Remove(genre);
    }

    public void AddPlatform(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        if (_platforms.Contains(platform))
            return;

        _platforms.Add(platform);
    }

    public void RemovePlatform(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        _platforms.Remove(platform);
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Title = title.Trim();
    }

    private void SetShortDescription(string shortDescription)
    {
        if (string.IsNullOrWhiteSpace(shortDescription))
            throw new ArgumentException("Short description cannot be empty.", nameof(shortDescription));

        ShortDescription = shortDescription.Trim();
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Description = description.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Price = price;
    }
}