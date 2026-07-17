using GameHub.Domain.Core;

namespace GameHub.Domain.Games;

public sealed class Platform : BaseEntity
{
    public string Name { get; private set; }

    protected Platform()
    {
        Name = string.Empty;
    }

    public Platform(string name)
    {
        SetName(name);
    }

    public void Rename(string name)
    {
        SetName(name);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Platform name cannot be empty.", nameof(name));

        Name = name.Trim();
    }
}