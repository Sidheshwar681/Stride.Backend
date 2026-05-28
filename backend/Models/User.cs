namespace Stride.Api.Models;

public sealed class User
{
    public Guid Id { get; init; }
    public string Username { get; init; } = "";
    public string Email { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; }
}

