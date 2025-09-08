// src/API/Controllers/OrdersController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;   // IOrderService, CreateOrderRequest
using Common;                 // DomainException

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CLIENTE")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto dto, CancellationToken ct)
    {
        // Extrai customerId do token (aceita variações)
        string? rawCustomerId =
            User.FindFirst("customerId")?.Value ??
            User.FindFirst("CustomerId")?.Value ??
            User.FindFirst("customerid")?.Value;

        if (string.IsNullOrWhiteSpace(rawCustomerId) || !Guid.TryParse(rawCustomerId, out var customerId) || customerId == Guid.Empty)
            return Unauthorized("Token sem customerId válido.");

        var req = new CreateOrderRequest
        {
            ProductId = dto.ProductId,
            Quantity  = dto.Quantity,
            Cep       = dto.Cep?.Replace("-", "").Trim(),
            AddressOverride = dto.AddressOverride
        };

        try
        {
            var orderId = await _orders.CreateOrderAsync(req, customerId, ct);
            // Retorna 201 com Location sem depender de GetById
            var location = Url.Content($"~/api/orders/{orderId}");
            return Created(location ?? $"/api/orders/{orderId}", new { id = orderId });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// DTO do controller
public sealed class CreateOrderRequestDto
{
    public Guid   ProductId        { get; set; }
    public int    Quantity         { get; set; }
    public string? Cep             { get; set; }
    public string? AddressOverride { get; set; }
}
