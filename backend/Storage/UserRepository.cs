using Microsoft.EntityFrameworkCore;
using Stride.Api.Data;
using Stride.Api.Models;
using Stride.Api.Services;
using Microsoft.Extensions.Logging;

namespace Stride.Api.Storage;

public sealed class UserRepository
{
    private readonly AppDbContext _context;
    private readonly IClock _clock;
    private readonly PasswordHasher _passwordHasher;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, IClock clock, PasswordHasher passwordHasher, ILogger<UserRepository> logger)
    {
        _context = context;
        _clock = clock;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> FindByEmailOrUsernameAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        var value = (identifier ?? "").Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return await _context.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == value.ToLower()
              || u.Username.ToLower() == value.ToLower(),
            cancellationToken);
    }

    public async Task<(User? User, string? Error)> CreateAsync(
        string username,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedUsername = (username ?? "").Trim();
        var normalizedEmail = (email ?? "").Trim();

        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return (null, "Username is required.");
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail) ||
            !normalizedEmail.Contains('@'))
        {
            return (null, "A valid email is required.");
        }

        var emailExists = await _context.Users.AnyAsync(
            u => u.Email.ToLower() == normalizedEmail.ToLower(),
            cancellationToken);

        if (emailExists)
        {
            return (null, "Email is already registered.");
        }

        var usernameExists = await _context.Users.AnyAsync(
            u => u.Username.ToLower() == normalizedUsername.ToLower(),
            cancellationToken);

        if (usernameExists)
        {
            return (null, "Username is already taken.");
        }

        var hash = _passwordHasher.Hash(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = hash,
            CreatedAt = _clock.UtcNow
        };

        _context.Users.Add(user);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            sw.Stop();
            _logger?.LogInformation("SaveChangesAsync took {ms}ms for user {email}", sw.ElapsedMilliseconds, normalizedEmail);
        }

        return (user, null);
    }
}