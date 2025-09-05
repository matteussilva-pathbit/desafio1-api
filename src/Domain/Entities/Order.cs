using System;
using Common;

namespace Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantidade { get; private set; }
        public decimal Preco { get; private set; }
        public string EnderecoEntrega { get; private set; } = default!;
        public DateTime DataPedido { get; private set; }
        public string Status { get; private set; } = default!;

        private Order() { } // EF Core

        public static Order Create(Guid customerId, Guid productId, int quantidade, decimal preco, string enderecoEntrega)
        {
            if (quantidade <= 0)
                throw new DomainException("A quantidade deve ser maior que zero.");
            if (preco <= 0)
                throw new DomainException("O preço deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(enderecoEntrega))
                throw new DomainException("O endereço de entrega é obrigatório.");

            return new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ProductId = productId,
                Quantidade = quantidade,
                Preco = preco,
                EnderecoEntrega = enderecoEntrega,
                DataPedido = DateTime.UtcNow,
                Status = "ENVIADO"
            };
        }
    }
}
