using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stride.Api.Models;
using Stride.Api.Storage;

namespace Stride.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/purchases")]
public sealed class PurchasesController : ControllerBase
{
    private readonly PurchaseRepository _purchases;

    public PurchasesController(PurchaseRepository purchases)
    {
        _purchases = purchases;
    }

    public sealed class PurchaseItemRequest
    {
        [Required, MinLength(1), MaxLength(80)]
        public string ProductId { get; init; } = "";

        [Required, MinLength(1), MaxLength(120)]
        public string Name { get; init; } = "";

        [Range(1, 99)]
        public int Quantity { get; init; }

        [Range(typeof(decimal), "0.01", "100000")]
        public decimal UnitPrice { get; init; }

        public string? Size { get; init; }
    }

    public sealed class CreatePurchaseRequest
    {
        [Required, MinLength(1)]
        public List<PurchaseItemRequest> Items { get; init; } = new();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Purchase>>> List(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var purchases = await _purchases.ListForUserAsync(userId.Value, cancellationToken);
        return Ok(purchases);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Purchase>> Get(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var purchase = await _purchases.FindForUserAsync(userId.Value, id, cancellationToken);
        if (purchase is null)
        {
            return NotFound();
        }

        return Ok(purchase);
    }

    [HttpPost]
    public async Task<ActionResult<Purchase>> Create(CreatePurchaseRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var username = User.FindFirst("username")?.Value;
        if (userId is null || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized();
        }

        if (request.Items.Count == 0)
        {
            return BadRequest(new { message = "At least one item is required." });
        }

        var items = request.Items.Select(i => new PurchaseItem
        {
            ProductId = i.ProductId.Trim(),
            Name = i.Name.Trim(),
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Size = string.IsNullOrWhiteSpace(i.Size) ? null : i.Size.Trim(),
        }).ToList();

        var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var total = subtotal;

        var purchase = await _purchases.CreateAsync(userId.Value, email, username, items, subtotal, total, cancellationToken);
        return Ok(purchase);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
