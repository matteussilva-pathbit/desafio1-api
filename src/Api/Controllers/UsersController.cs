using System.ComponentModel.DataAnnotations;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Repositories; // <- adiciona esta linha


namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ADMIN")]
public sealed class UsersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IUnitOfWork _uow;

    public UsersController(AppDbContext ctx, IUnitOfWork uow)
    {
        _ctx = ctx;
        _uow = uow;
    }

    // GET /api/users?skip=0&take=20&search=admin
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (take is < 1 or > 100) take = 20;

        var query = _ctx.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(u =>
                EF.Functions.ILike(u.Email, $"%{s}%") ||
                EF.Functions.ILike(u.UserName, $"%{s}%") ||
                EF.Functions.ILike(u.Type, $"%{s}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.UserName)
            .Skip(skip)
            .Take(take)
            .Select(u => new UserResponse(u.Id, u.Email, u.UserName, u.Type, u.CustomerId))
            .ToListAsync(ct);

        return Ok(new PagedResult<UserResponse>(items, total, skip, take));
    }

    // GET /api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct)
    {
        var u = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u is null) return NotFound();

        return Ok(new UserResponse(u.Id, u.Email, u.UserName, u.Type, u.CustomerId));
    }

    // DELETE /api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _ctx.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound();

        // (opcional) impedir apagar o próprio admin logado
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != null && Guid.TryParse(currentUserId, out var me) && me == id)
            return BadRequest(new { message = "Você não pode excluir o próprio usuário logado." });

        _ctx.Users.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return NoContent();
    }

    public sealed record UserResponse(Guid Id, string Email, string UserName, string Type, Guid? CustomerId);
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);
}
