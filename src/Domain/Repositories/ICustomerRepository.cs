using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer, CancellationToken ct);
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Customer?> GetByEmailAsync(string email, CancellationToken ct);
        Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct);
    }
}
