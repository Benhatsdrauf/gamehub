namespace GameHub.Application.Users.GetUsers;

// The per-row shape for the user list. A separate, lighter DTO than GetUserResponse
// so the list view can evolve independently of the single-user view.
public sealed record UserListItem(
    Guid Id,
    string Username,
    string Email);
