using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;           // IOrderService
using Application.DTOs.Orders;         // se preferir usar seu DTO global (opcional)
using Domain.Entities;                 // Order/OrderItem (para listar)
using Domain.Enums;                    // OrderStatus (se usar em listagens/filtros)
using Infrastructure.Data;             // AppDbContext (para listagens)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize] // <- só Bearer. Policies por ação (Admin/Customer)
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IOrderService _orders;

    public OrdersController(AppDbContext ctx, IOrderService orders)
    {
        _ctx = ctx;
        _orders = orders;
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
        var cidStr = User.FindFirst("customerId")?.Value; // mantém seu nome de claim
        if (!Guid.TryParse(cidStr, out var cid)) return Unauthorized("Token sem customerId válido.");

        var list = await _ctx.Orders
            .Where(o => o.CustomerId == cid)
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(list);
    }

    // DTO local para criação rápida (se já tiver um DTO global, pode usar ele e remover esta classe)
    public class OrderCreateRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string Cep { get; set; } = string.Empty;
        public string? DeliveryAddressOverride { get; set; } // ignorado no controller; service valida CEP
    }

    // Cliente cria order (service valida CEP, estoque e transação)
    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequest req, CancellationToken ct)
    {
        if (req.Quantity <= 0) return BadRequest("Quantidade inválida.");

        var cidStr = User.FindFirst("customerId")?.Value; // mantém seu nome de claim
        if (!Guid.TryParse(cidStr, out var customerId)) return Unauthorized("Token sem customerId válido.");

        // toda regra (ViaCEP, estoque, transação) fica no service:
        var orderId = await _orders.CreateAsync(customerId, req.ProductId, req.Quantity, req.Cep, ct);

        // opcional: buscar a ordem criada para devolver no corpo
        var created = await _ctx.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        // Sem rota específica de GetById, usamos Created com a URL canônica:
        return Created($"/api/orders/{orderId}", created is not null ? (object)created : new { id = orderId });

    }
}
