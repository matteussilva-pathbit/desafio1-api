using System.Security.Claims;
using Application.DTOs.Orders;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    public OrdersController(IOrderService svc) => _svc = svc;

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer,Basic", Policy="Customer")]
    public async Task<IActionResult> Create([FromBody] OrderCreateDto dto, CancellationToken ct)
    {
        var customerIdStr = User.FindFirstValue("customerId");
        if (string.IsNullOrWhiteSpace(customerIdStr)) return Unauthorized();
        if (!Guid.TryParse(customerIdStr, out var customerId)) return Unauthorized();

        var id = await _svc.CreateAsync(dto, customerId, ct);
        return id is null ? BadRequest("Falha na validação (CEP/produto/quantidade).") : CreatedAtAction(null, new { id }, null);
    }
}
