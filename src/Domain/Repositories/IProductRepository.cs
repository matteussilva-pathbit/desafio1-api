using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
        Task AddAsync(Product product, CancellationToken ct);
        void Update(Product product);
    }
}
