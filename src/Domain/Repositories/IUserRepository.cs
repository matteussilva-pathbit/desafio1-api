using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task AddAsync(User user, CancellationToken ct);
    }
}
