using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface;   // IViaCepService
using Common;                 // DomainException
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(CreateOrderRequest request, Guid customerId, CancellationToken ct);
    }

    public sealed class CreateOrderRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string Cep { get; set; } = default!;
        public string? AddressOverride { get; set; }
    }

    public sealed class OrderService : IOrderService
    {
        private readonly IOrderRepository _orders;
        private readonly IProductRepository _products;
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _uow;
        private readonly IViaCepService _viaCep;

        public OrderService(
            IOrderRepository orders,
            IProductRepository products,
            ICustomerRepository customers,
            IUnitOfWork uow,
            IViaCepService viaCep)
        {
            _orders = orders;
            _products = products;
            _customers = customers;
            _uow = uow;
            _viaCep = viaCep;
        }

        public async Task<Guid> CreateOrderAsync(CreateOrderRequest request, Guid customerId, CancellationToken ct)
        {
            if (request.Quantity <= 0)
                throw new DomainException("Quantidade deve ser > 0.");

            var customer = await _customers.GetByIdAsync(customerId, ct)
                           ?? throw new DomainException("Cliente inválido.");

            var product = await _products.GetByIdAsync(request.ProductId, ct)
                          ?? throw new DomainException("Produto não encontrado.");

            if (product.QuantityAvailable < request.Quantity)
                throw new DomainException("Estoque insuficiente.");

            // Resolução de endereço
            var endereco = request.AddressOverride;
            if (string.IsNullOrWhiteSpace(endereco))
            {
                var cepInfo = await _viaCep.BuscarEnderecoAsync(request.Cep, ct);
                if (cepInfo is null)
                    throw new DomainException("CEP inválido ou não encontrado.");
                endereco = $"{cepInfo.Logradouro}, {cepInfo.Bairro}, {cepInfo.Localidade}-{cepInfo.Uf}";
            }

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // Debita estoque
                product.DebitInventory(request.Quantity);
                _products.Update(product);

                // Cria pedido (atenção aos nomes dos parâmetros da entidade Order)
                var order = Order.Create(
                    customerId: customer.Id,
                    productId: product.Id,
                    quantidade: request.Quantity,
                    preco: product.Preco,
                    enderecoEntrega: endereco!
                );

                await _orders.AddAsync(order, ct);
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
