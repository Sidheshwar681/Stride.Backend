namespace Stride.Api.Models;

public sealed class PurchaseItem
{
    public Guid Id { get; set; }

    public string ProductId { get; set; } = "";

    public string Name { get; set; } = "";

    public string? Size { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public Guid PurchaseId { get; set; }

    public Purchase? Purchase { get; set; }
}