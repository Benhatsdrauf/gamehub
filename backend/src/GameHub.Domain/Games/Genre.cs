using GameHub.Domain.Core;

namespace GameHub.Domain.Games;

public sealed class Genre : BaseEntity
{
    public string Name { get; private set; }

    // Required by EF Core
    protected Genre()
    {
        Name = string.Empty;
    }

    public Genre(string name)
    {
        SetName(name);
    }

    public void Rename(string newName)
    {
        SetName(newName);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Genre name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }
}