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
[Route("api/customers")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public CustomersController(AppDbContext ctx) => _ctx = ctx;

    // Admin lista todos
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll(CancellationToken ct)
        => Ok(await _ctx.Customers.AsNoTracking().ToListAsync(ct));

    // Admin/Cliente busca por id (admin qualquer; cliente só o próprio id)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("ADMINISTRADOR");
        if (!isAdmin)
        {
            var cidStr = User.FindFirst("customerId")?.Value;
            if (!Guid.TryParse(cidStr, out var cid) || cid != id) return Forbid();
        }

        var c = await _ctx.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return c is null ? NotFound() : Ok(c);
    }

    // Cliente pega o próprio cadastro
    [HttpGet("me")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var cidStr = User.FindFirst("customerId")?.Value;
        if (!Guid.TryParse(cidStr, out var cid)) return Unauthorized();

        var c = await _ctx.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cid, ct);
        return c is null ? NotFound() : Ok(c);
    }

    // Cliente atualiza o próprio cadastro
    [HttpPut("me")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> UpdateMe([FromBody] Customer dto, CancellationToken ct)
    {
        var cidStr = User.FindFirst("customerId")?.Value;
        if (!Guid.TryParse(cidStr, out var cid)) return Unauthorized();

        var c = await _ctx.Customers.FirstOrDefaultAsync(x => x.Id == cid, ct);
        if (c is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email é obrigatório.");

        c.Name = dto.Name;
        c.Email = dto.Email;

        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }

    // Admin pode atualizar qualquer cliente
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Customer dto, CancellationToken ct)
    {
        var c = await _ctx.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email é obrigatório.");

        c.Name = dto.Name;
        c.Email = dto.Email;

        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }

    // Admin pode remover
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var c = await _ctx.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return NotFound();

        _ctx.Customers.Remove(c);
        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }
}
