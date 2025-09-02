using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;   // IOrderService, IViaCepService
using Domain.Repositories;     // IOrderRepository, IProductRepository (se você usar)
using Domain.Entities;         // Order

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orders;
        private readonly IProductRepository _products;
        private readonly IViaCepService _cep;

        public OrderService(IOrderRepository orders, IProductRepository products, IViaCepService cep)
        {
            _orders = orders;
            _products = products;
            _cep = cep;
        }

        // Retorna Guid para casar com seu Domain.Entities.Order.Id
        public async Task<Guid> CreateAsync(Guid customerId, Guid productId, int quantity, string cep, CancellationToken ct = default)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser > 0.", nameof(quantity));

            if (string.IsNullOrWhiteSpace(cep))
                throw new ArgumentException("CEP é obrigatório.", nameof(cep));

            // 1) Valida CEP via ViaCEP (não persistimos endereço, só validamos conforme regra)
            var viaCep = await _cep.GetAsync(cep, ct);
            if (viaCep is null || viaCep.erro == true || string.IsNullOrWhiteSpace(viaCep.logradouro))
                throw new InvalidOperationException("CEP inválido ou não encontrado no ViaCEP.");

            // 2) Transação: valida produto, debita estoque e cria o pedido
            await _orders.BeginTransactionAsync(ct);
            try
            {
                var product = await _orders.GetProductForUpdateAsync(productId, ct);
                if (product is null)
                    throw new InvalidOperationException("Produto não existe.");

                if (product.QuantityAvailable < quantity)
                    throw new InvalidOperationException("Quantidade indisponível para o produto.");

                // Debita estoque
                product.QuantityAvailable -= quantity;

                // Cria o pedido (somente o que existe no seu domínio)
                var order = new Order
                {
                    // Seu Order **não** tem ProductId/Quantity/Shipping*, então não setamos.
                    // Presumindo que exista CustomerId no Order:
                    CustomerId = customerId
                    // Se seu Order tiver Status/CreatedAt/etc., defina aqui.
                };

                // Se o seu domínio exigir OrderItems:
                // - crie e associe um OrderItem aqui (ajuste nomes conforme seu OrderItem):
                // order.Items = new List<OrderItem> { new OrderItem { ProductId = productId, Quantity = quantity, ... } };

                await _orders.AddAsync(order, ct);
                await _orders.SaveChangesAsync(ct);
                await _orders.CommitAsync(ct);

                return order.Id; // Guid
            }
            catch
            {
                await _orders.RollbackAsync(ct);
                throw;
            }
        }
    }
}
