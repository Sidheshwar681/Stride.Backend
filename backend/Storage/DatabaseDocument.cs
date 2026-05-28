using Stride.Api.Models;

namespace Stride.Api.Storage;

public sealed class DatabaseDocument
{
    public int SchemaVersion { get; init; } = 1;
    public List<User> Users { get; init; } = new();
    public List<Purchase> Purchases { get; init; } = new();
}

