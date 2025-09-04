using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Services; // IOrderService
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders)
        {
            _orders = orders;
        }

        public sealed class CreateOrderRequestDto
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
            public string Cep { get; set; } = default!;
            public string? AddressOverride { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto dto, CancellationToken ct)
        {
            // No futuro: pegar customerId do JWT. Por agora, aceite via header para testar.
            // Exemplo: x-customer-id: GUID
            var headerId = Request.Headers["x-customer-id"].ToString();
            if (!Guid.TryParse(headerId, out var customerId) || customerId == Guid.Empty)
                return BadRequest("x-customer-id inválido. (No futuro virá do JWT)");

            var id = await _orders.CreateOrderAsync(new Application.Services.CreateOrderRequest
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Cep = dto.Cep,
                AddressOverride = dto.AddressOverride
            }, customerId, ct);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // apenas um stub para a rota do CreatedAtAction
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id) => Ok(new { id });
    }
}
