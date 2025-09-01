using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ProductsController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(CancellationToken ct)
        => Ok(await _ctx.Products.AsNoTracking().ToListAsync(ct));

    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    public async Task<ActionResult<Product>> GetById([FromRoute] Guid id, CancellationToken ct)
        => await _ctx.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct) is { } p ? Ok(p) : NotFound();

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer,Basic", Policy = "Admin")]
    public async Task<IActionResult> Create([FromBody] Product dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Nome é obrigatório.");
        if (dto.Price < 0) return BadRequest("Preço inválido.");
        if (dto.QuantityAvailable <= 0) return BadRequest("Quantidade deve ser > 0.");

        _ctx.Products.Add(dto);
        await _ctx.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic", Policy = "Admin")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Product dto, CancellationToken ct)
    {
        var p = await _ctx.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Nome é obrigatório.");
        if (dto.Price < 0) return BadRequest("Preço inválido.");
        if (dto.QuantityAvailable < 0) return BadRequest("Quantidade não pode ser negativa.");

        p.Name = dto.Name;
        p.Price = dto.Price;
        p.QuantityAvailable = dto.QuantityAvailable;

        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic", Policy = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var p = await _ctx.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        _ctx.Products.Remove(p);
        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }
}
