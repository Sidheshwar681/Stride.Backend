namespace Stride.Api.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Stride.Api";
    public string Audience { get; init; } = "Stride.Web";

    // NOTE: Set this in appsettings.json; do not hardcode in source.
    public string SigningKey { get; init; } = "";

    public int AccessTokenMinutes { get; init; } = 120;
}

