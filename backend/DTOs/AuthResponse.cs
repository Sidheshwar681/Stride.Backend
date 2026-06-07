namespace Stride.Api.DTOs;

public sealed record AuthResponse(
    Guid UserId,
    string Username,
    string Email,
    string AccessToken);