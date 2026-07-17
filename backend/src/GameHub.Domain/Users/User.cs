using GameHub.Domain.Core;
using GameHub.Domain.Enums;

namespace GameHub.Domain.Users;

public sealed class User : BaseEntity
{
    public string Username { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string? AvatarPath { get; private set; }

    public UserRole Role { get; private set; }

    public DeveloperProfile? DeveloperProfile { get; private set; }

    protected User()
    {
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(
        string username,
        string email,
        string passwordHash)
    {
        SetUsername(username);
        SetEmail(email);
        SetPasswordHash(passwordHash);

        Role = UserRole.User;
    }

    public void Rename(string username)
    {
        SetUsername(username);
    }

    public void UpdateEmail(string email)
    {
        SetEmail(email);
    }

    public void UpdatePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
    }

    public void UpdateAvatar(string? avatarPath)
    {
        AvatarPath = string.IsNullOrWhiteSpace(avatarPath)
            ? null
            : avatarPath.Trim();
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
    }

    public void CreateDeveloperProfile(DeveloperProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (DeveloperProfile is not null)
            throw new InvalidOperationException("User already has a developer profile.");

        DeveloperProfile = profile;
    }

    private void SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        Username = username.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        Email = email.Trim();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }
}