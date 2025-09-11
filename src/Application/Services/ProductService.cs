using System;
using System.Threading;
using System.Threading.Tasks;
using Common;                          // DomainException
using Domain.Entities;                 // Product
using Domain.Repositories;             // IProductRepository, IUnitOfWork

namespace Application.Services
{
    // OBS: Não implementa nenhuma interface de Application.Interfaces
    // porque o contrato oficial usa int + DTOs e sua service usa Guid e regras de negócio específicas.
    public sealed class ProductService
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
            if (newPrice <= 0)
                throw new DomainException("Preço inválido.");

            var product = await _repo.GetByIdAsync(id, ct);
            if (product is null)
                throw new DomainException("Produto não encontrado.");

            product.UpdatePrice(newPrice);
            _repo.Update(product);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task AdjustInventoryAsync(Guid id, int delta, CancellationToken ct)
        {
            var product = await _repo.GetByIdAsync(id, ct);
            if (product is null)
                throw new DomainException("Produto não encontrado.");

            if (delta == 0)
                return;

            if (delta > 0)
                product.IncreaseInventory(delta);
            else
                product.DebitInventory(-delta);

            _repo.Update(product);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
