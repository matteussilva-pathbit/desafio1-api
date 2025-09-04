using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;

        public UserRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
            => _ctx.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
            => _ctx.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task AddAsync(User user, CancellationToken ct)
            => await _ctx.Users.AddAsync(user, ct);
    }
}
