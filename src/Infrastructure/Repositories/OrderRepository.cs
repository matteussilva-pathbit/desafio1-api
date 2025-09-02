using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task BeginTransactionAsync(CancellationToken ct = default)
            => _db.Database.BeginTransactionAsync(ct);

        public Task CommitAsync(CancellationToken ct = default)
            => _db.Database.CommitTransactionAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default)
            => _db.Database.RollbackTransactionAsync(ct);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public async Task<Order> AddAsync(Order order, CancellationToken ct = default)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);
            return order;
        }

        public Task<Product?> GetProductForUpdateAsync(Guid productId, CancellationToken ct = default)
        {
            // Se você usa concorrência otimista, considere AsTracking() (já é default ao consultar DbSet<T>)
            return _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        }
    }
}
