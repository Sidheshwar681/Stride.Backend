namespace Stride.Api.DTOs;

public sealed record MeResponse(
    Guid UserId,
    string Username,
    string Email,
    DateTimeOffset CreatedAt);