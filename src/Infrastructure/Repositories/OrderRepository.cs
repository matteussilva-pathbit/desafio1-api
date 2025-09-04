using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _ctx;
        public OrderRepository(AppDbContext ctx) => _ctx = ctx;

        public Task AddAsync(Order order, CancellationToken ct)
            => _ctx.Orders.AddAsync(order, ct).AsTask();
    }
}
