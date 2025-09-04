using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _ctx;
        public CustomerRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task AddAsync(Customer customer, CancellationToken ct)
            => await _ctx.Customers.AddAsync(customer, ct);

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct)
            => _ctx.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

        public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct)
            => _ctx.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Email == email, ct);

        public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct)
            => await _ctx.Customers.AsNoTracking().ToListAsync(ct);
    }
}
