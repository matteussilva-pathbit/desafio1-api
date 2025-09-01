using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
