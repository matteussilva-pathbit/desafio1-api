using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface IOrderRepository
    {
        // Transação no mesmo DbContext (para garantir consistência ao debitar estoque e criar o pedido)
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        // Persistência do pedido
        Task<Order> AddAsync(Order order, CancellationToken ct = default);

        // Carregar produto para débito de estoque dentro da mesma transação
        Task<Product?> GetProductForUpdateAsync(Guid productId, CancellationToken ct = default);
    }
}
