using Microsoft.EntityFrameworkCore;
using Stride.Api.Data;
using Stride.Api.Models;
using Stride.Api.Services;

namespace Stride.Api.Storage;

public sealed class UserRepository
{
    private readonly AppDbContext _context;
    private readonly IClock _clock;

    public UserRepository(AppDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
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
        string passwordHash,
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

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            CreatedAt = _clock.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync(cancellationToken);

        return (user, null);
    }
}