using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface;       // IViaCepService
using Domain;                      // DomainException
using Domain.Entities;             // Order
using Domain.Repositories;         // IProductRepository, IOrderRepository, IUnitOfWork

namespace Application.Services
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(CreateOrderRequest request, Guid customerId, CancellationToken ct);
    }

    // DTO simples de criação — ajuste se você já tiver outro DTO
    public sealed class CreateOrderRequest
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
        public string Cep { get; init; } = default!;
        public string? AddressOverride { get; init; }
    }

    public sealed class OrderService : IOrderService
    {
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IUnitOfWork _uow;
        private readonly IViaCepService _viaCep;

        public OrderService(
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IUnitOfWork uow,
            IViaCepService viaCep)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _uow = uow;
            _viaCep = viaCep;
        }

        public async Task<Guid> CreateOrderAsync(CreateOrderRequest request, Guid customerId, CancellationToken ct)
        {
            if (customerId == Guid.Empty)
                throw new DomainException("CustomerId inválido.");

            var product = await _productRepo.GetByIdAsync(request.ProductId, ct);
            if (product is null)
                throw new DomainException("Produto não encontrado.");

            if (product.QuantityAvailable <= 0)
                throw new DomainException("Produto sem estoque disponível.");

            if (product.QuantityAvailable < request.Quantity)
                throw new DomainException("Quantidade solicitada maior que o estoque disponível.");

            // Endereço: override explícito ou ViaCEP
            var enderecoCompleto = !string.IsNullOrWhiteSpace(request.AddressOverride)
                ? request.AddressOverride!.Trim()
                : await _viaCep.GetEnderecoByCepAsync(request.Cep, ct);

            if (string.IsNullOrWhiteSpace(enderecoCompleto))
                throw new DomainException("CEP inválido ou endereço não encontrado.");

            var order = Order.Create(
                customerId: customerId,
                productId: product.Id,
                quantidade: request.Quantity,
                precoUnitario: product.Preco,
                cep: request.Cep,
                endereco: enderecoCompleto
            );

            product.DebitarEstoque(request.Quantity);

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _orderRepo.AddAsync(order, ct);
                _productRepo.Update(product);

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);

                return order.Id;
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }
    }
}
