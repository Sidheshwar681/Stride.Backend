namespace Stride.Api.Services;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

