using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _ctx;
    public UserRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken ct = default)
        => _ctx.Users.Include(u => u.Customer)
                     .FirstOrDefaultAsync(u => u.UserName == userNameOrEmail || u.Email == userNameOrEmail, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        _ctx.Users.Add(user);
        await _ctx.SaveChangesAsync(ct);
    }
}
