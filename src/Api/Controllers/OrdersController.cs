using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// alias para a interface "oficial" da camada Application.Interface
using IOrderServiceApp = Application.Interface.IOrderService;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CLIENTE")]
public class OrdersController : ControllerBase
{
    private readonly IOrderServiceApp _orderService;

    public OrdersController(IOrderServiceApp orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto dto, CancellationToken ct)
    {
        // Extrai customerId do token (aceita variações de claim)
        string? rawCustomerId =
            User.FindFirst("customerId")?.Value ??
            User.FindFirst("CustomerId")?.Value ??
            User.FindFirst("customerid")?.Value;

        if (string.IsNullOrWhiteSpace(rawCustomerId) ||
            !Guid.TryParse(rawCustomerId, out var customerId) ||
            customerId == Guid.Empty)
        {
            return Unauthorized("Token sem customerId válido.");
        }

        // Normaliza CEP (remove '-')
        var cep = (dto.Cep ?? string.Empty).Replace("-", "").Trim();

        try
        {
            // Usa o contrato da interface oficial
            var orderId = await _orderService.CreateAsync(
                customerId: customerId,
                productId: dto.ProductId,
                quantity: dto.Quantity,
                cep: cep,
                ct: ct
            );

            var location = Url.Content($"~/api/orders/{orderId}");
            return Created(location ?? $"/api/orders/{orderId}", new { id = orderId });
        }
        catch (Common.DomainException ex)
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
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Cep { get; set; }

    // Se quiser suportar AddressOverride via Controller,
    // precisaria expor isso também no IOrderService (Application.Interface).
    public string? AddressOverride { get; set; }
}
