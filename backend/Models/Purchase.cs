namespace Stride.Api.Models;

public sealed class Purchase
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string UserEmail { get; set; } = "";

    public string UserUsername { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public List<PurchaseItem> Items { get; set; } = new();
}