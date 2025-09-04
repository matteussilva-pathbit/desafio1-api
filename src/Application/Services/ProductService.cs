using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;                 // DomainException
using Domain.Entities;        // Product
using Domain.Repositories;    // IProductRepository, IUnitOfWork

namespace Application.Services
{
    public interface IProductService
    {
        Task<Product> GetByIdAsync(Guid id, CancellationToken ct);
        Task UpdatePriceAsync(Guid id, decimal newPrice, CancellationToken ct);
        Task AdjustInventoryAsync(Guid id, int quantityDelta, CancellationToken ct);
    }

    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _uow;

        public ProductService(IProductRepository productRepo, IUnitOfWork uow)
        {
            _productRepo = productRepo;
            _uow = uow;
        }

        public async Task<Product> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var product = await _productRepo.GetByIdAsync(id, ct);
            if (product is null) throw new DomainException("Produto não encontrado.");
            return product;
        }

        public async Task UpdatePriceAsync(Guid id, decimal newPrice, CancellationToken ct)
        {
            if (newPrice <= 0) throw new DomainException("Preço deve ser maior que zero.");

            var product = await _productRepo.GetByIdAsync(id, ct);
            if (product is null) throw new DomainException("Produto não encontrado.");

            // Como a entidade não tinha um setter público para preço,
            // vamos recriar a regra de atualização da forma mais simples:
            // (Se você preferir, adicione um método de domínio Product.AlterarPreco)
            var updated = Product.Create(product.Nome, newPrice, product.QuantityAvailable);
            typeof(Product).GetProperty(nameof(Product.Id))!
                           .SetValue(updated, product.Id);

            await _uow.BeginTransactionAsync(ct);
            try
            {
                _productRepo.Update(updated);
                await _uow.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task AdjustInventoryAsync(Guid id, int quantityDelta, CancellationToken ct)
        {
            var product = await _productRepo.GetByIdAsync(id, ct);
            if (product is null) throw new DomainException("Produto não encontrado.");

            // delta positivo = entrada de estoque; negativo = saída de estoque
            if (quantityDelta == 0) return;

            if (quantityDelta > 0)
            {
                // como não há método para adicionar estoque, fazemos uma atualização direta
                var newQty = product.QuantityAvailable + quantityDelta;
                var updated = Product.Create(product.Nome, product.Preco, newQty);
                typeof(Product).GetProperty(nameof(Product.Id))!
                               .SetValue(updated, product.Id);

                await _uow.BeginTransactionAsync(ct);
                try
                {
                    _productRepo.Update(updated);
                    await _uow.SaveChangesAsync(ct);
                    await _uow.CommitAsync(ct);
                }
                catch
                {
                    await _uow.RollbackAsync(ct);
                    throw;
                }
            }
            else
            {
                // usar a regra de domínio para “debitar” (garante não ficar negativo)
                var toDebit = Math.Abs(quantityDelta);

                await _uow.BeginTransactionAsync(ct);
                try
                {
                    product.DebitarEstoque(toDebit);
                    _productRepo.Update(product);
                    await _uow.SaveChangesAsync(ct);
                    await _uow.CommitAsync(ct);
                }
                catch
                {
                    await _uow.RollbackAsync(ct);
                    throw;
                }
            }
        }
    }
}
