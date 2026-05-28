using Microsoft.EntityFrameworkCore;
using Stride.Api.Data;
using Stride.Api.Models;
using Stride.Api.Services;

namespace Stride.Api.Storage;

public sealed class PurchaseRepository
{
    private readonly AppDbContext _context;
    private readonly IClock _clock;

    public PurchaseRepository(AppDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<IReadOnlyList<Purchase>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.Purchases
            .Include(p => p.Items)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Purchase?> FindForUserAsync(
        Guid userId,
        Guid purchaseId,
        CancellationToken cancellationToken)
    {
        return await _context.Purchases
            .Include(p => p.Items)
            .FirstOrDefaultAsync(
                p => p.UserId == userId && p.Id == purchaseId,
                cancellationToken);
    }

    public async Task<Purchase> CreateAsync(
        Guid userId,
        string userEmail,
        string userUsername,
        List<PurchaseItem> items,
        decimal subtotal,
        decimal total,
        CancellationToken cancellationToken)
    {
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail,
            UserUsername = userUsername,
            CreatedAt = _clock.UtcNow,
            Items = items,
            Subtotal = subtotal,
            Total = total
        };

        _context.Purchases.Add(purchase);

        await _context.SaveChangesAsync(cancellationToken);

        return purchase;
    }
}