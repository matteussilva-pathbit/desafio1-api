using System;
using System.Threading;
using System.Threading.Tasks;
using Common;


namespace Application.Services
{
    public interface IProductService
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
        Task UpdatePriceAsync(Guid id, decimal newPrice, CancellationToken ct);
        Task AdjustInventoryAsync(Guid id, int delta, CancellationToken ct);
    }

    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;

        public ProductService(IProductRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
            => _repo.GetByIdAsync(id, ct);

        public async Task UpdatePriceAsync(Guid id, decimal newPrice, CancellationToken ct)
        {
            if (newPrice <= 0) throw new DomainException("Preço inválido.");

            var product = await _repo.GetByIdAsync(id, ct);
            if (product is null) throw new DomainException("Produto não encontrado.");

            product.UpdatePrice(newPrice);
            _repo.Update(product);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task AdjustInventoryAsync(Guid id, int delta, CancellationToken ct)
        {
            var product = await _repo.GetByIdAsync(id, ct);
            if (product is null) throw new global::Common.DomainException("Produto não encontrado.");

            if (delta == 0) return;

            if (delta > 0)
                product.IncreaseInventory(delta);
            else
                product.DebitInventory(-delta);

            _repo.Update(product);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
