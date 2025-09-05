using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Services; // IOrderService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders) => _orders = orders;

        public sealed class CreateOrderRequestDto
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
            public string Cep { get; set; } = default!;
            public string? AddressOverride { get; set; }
        }

        [Authorize(Policy = "CLIENTE")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto dto, CancellationToken ct)
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(sub, out var customerId) || customerId == Guid.Empty)
                return Unauthorized("Token sem 'sub' válido.");

            var id = await _orders.CreateOrderAsync(new Application.Services.CreateOrderRequest
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Cep = dto.Cep,
                AddressOverride = dto.AddressOverride
            }, customerId, ct);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id) => Ok(new { id });
    }
}
