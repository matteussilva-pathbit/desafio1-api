using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IViaCepService _viaCep;

    public OrdersController(AppDbContext ctx, IViaCepService viaCep)
    {
        _ctx = ctx;
        _viaCep = viaCep;
    }

    // Admin lista todos
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(CancellationToken ct)
        => Ok(await _ctx.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync(ct));

    // Cliente lista suas próprias orders
    [HttpGet("mine")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var cidStr = User.FindFirst("customerId")?.Value;
        if (!Guid.TryParse(cidStr, out var cid)) return Unauthorized();

        var list = await _ctx.Orders
            .Where(o => o.CustomerId == cid)
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(list);
    }

    // Cliente cria order (com regras de CEP, estoque e transação)
    public class OrderCreateRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string Cep { get; set; } = string.Empty;
        public string? DeliveryAddressOverride { get; set; } // opcional: se quiser sobrescrever
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequest req, CancellationToken ct)
    {
        if (req.Quantity <= 0) return BadRequest("Quantidade inválida.");

        var cidStr = User.FindFirst("customerId")?.Value;
        if (!Guid.TryParse(cidStr, out var customerId)) return Unauthorized();

        var product = await _ctx.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId, ct);
        if (product is null) return BadRequest("Produto não existe.");

        if (product.QuantityAvailable < req.Quantity)
            return BadRequest("Estoque insuficiente.");

        // via CEP
        var addr = await _viaCep.GetAddressAsync(req.Cep, ct);
        if (addr is null) return BadRequest("CEP inválido ou não encontrado.");

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.ENVIADO,
            DeliveryZipCode = req.Cep,
            DeliveryAddress = req.DeliveryAddressOverride ?? $"{addr.Logradouro}, {addr.Bairro}, {addr.Localidade}-{addr.Uf}"
        };

        var item = new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = req.Quantity,
            UnitPrice = product.Price
        };

        order.Items = new List<OrderItem> { item };

        _ctx.Orders.Add(order);

        // atualiza estoque com segurança
        product.QuantityAvailable -= req.Quantity;

        await _ctx.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return CreatedAtAction(nameof(GetMine), new { }, order);
    }
}
