using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Repositories; // <- adiciona esta linha


namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IUnitOfWork _uow;

    public CustomersController(AppDbContext ctx, IUnitOfWork uow)
    {
        _ctx = ctx;
        _uow = uow;
    }

    // GET /api/customers?skip=0&take=20&search=ana
    [HttpGet]
    [Authorize(Policy = "ADMIN")]
    public async Task<ActionResult<PagedResult<CustomerResponse>>> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (take is < 1 or > 100) take = 20;

        var query = _ctx.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                EF.Functions.ILike(c.Nome, $"%{s}%") ||
                EF.Functions.ILike(c.Email, $"%{s}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Nome)
            .Skip(skip)
            .Take(take)
            .Select(c => new CustomerResponse(c.Id, c.Nome, c.Email))
            .ToListAsync(ct);

        return Ok(new PagedResult<CustomerResponse>(items, total, skip, take));
    }

    // GET /api/customers/{id}
    [HttpGet("{id:guid}")]
    [Authorize] // ADMIN ou CLIENTE (cliente poderia consultar o próprio perfil, se quiser)
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken ct)
    {
        var c = await _ctx.Customers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (c is null) return NotFound();

        return Ok(new CustomerResponse(c.Id, c.Nome, c.Email));
    }

    // POST /api/customers
    [HttpPost]
    [Authorize(Policy = "ADMIN")]
    public async Task<ActionResult<IdResponse>> Create([FromBody] CreateCustomerRequest req, CancellationToken ct)
    {
        // validação simples
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var exists = await _ctx.Customers.AsNoTracking()
            .AnyAsync(x => x.Email == req.Email, ct);
        if (exists) return Conflict(new { message = "E-mail já cadastrado." });

        var entity = Customer.Create(req.Nome, req.Email);

        await _ctx.Customers.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = entity.Id },
            new IdResponse(entity.Id)
        );
    }

    // PUT /api/customers/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ADMIN")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var entity = await _ctx.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound();

        // e-mail já usado por outro
        var emailUsed = await _ctx.Customers
            .AnyAsync(x => x.Email == req.Email && x.Id != id, ct);
        if (emailUsed) return Conflict(new { message = "E-mail já utilizado por outro cliente." });

        // A entidade tem setters privados; usamos o ChangeTracker para atualizar com segurança
        _ctx.Entry(entity).Property(nameof(Customer.Nome)).CurrentValue = req.Nome;
        _ctx.Entry(entity).Property(nameof(Customer.Email)).CurrentValue = req.Email;

        await _uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE /api/customers/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ADMIN")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _ctx.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound();

        _ctx.Customers.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // DTOs/Contracts (simples e locais ao controller)
    public sealed record CreateCustomerRequest(
        [property: Required, MaxLength(200)] string Nome,
        [property: Required, EmailAddress, MaxLength(200)] string Email);

    public sealed record UpdateCustomerRequest(
        [property: Required, MaxLength(200)] string Nome,
        [property: Required, EmailAddress, MaxLength(200)] string Email);

    public sealed record CustomerResponse(Guid Id, string Nome, string Email);

    public sealed record IdResponse(Guid Id);

    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);
}
