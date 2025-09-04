using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public DateTimeOffset DataPedido { get; private set; } = DateTimeOffset.UtcNow;
        public OrderStatus Status { get; private set; } = OrderStatus.Enviado;

        // FKs obrigatórias
        public Guid CustomerId { get; private set; }
        public Customer Customer { get; private set; } = default!;

        public Guid ProductId { get; private set; }
        public Product Product { get; private set; } = default!;

        public int Quantidade { get; private set; }
        public decimal PrecoUnitario { get; private set; }

        public string CepEntrega { get; private set; } = default!;
        public string EnderecoEntrega { get; private set; } = default!;

        private Order() { }

        private Order(Guid customerId, Guid productId, int quantidade, decimal precoUnitario, string cep, string endereco)
        {
            if (customerId == Guid.Empty) throw new DomainException("CustomerId obrigatório.");
            if (productId == Guid.Empty) throw new DomainException("ProductId obrigatório.");
            if (quantidade <= 0) throw new DomainException("Quantidade deve ser > 0.");
            if (precoUnitario <= 0) throw new DomainException("Preço unitário deve ser > 0.");
            if (string.IsNullOrWhiteSpace(cep)) throw new DomainException("CEP é obrigatório.");
            if (string.IsNullOrWhiteSpace(endereco)) throw new DomainException("Endereço é obrigatório.");

            CustomerId = customerId;
            ProductId = productId;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
            CepEntrega = cep.Trim();
            EnderecoEntrega = endereco.Trim();
        }

        public static Order Create(Guid customerId, Guid productId, int quantidade, decimal precoUnitario, string cep, string endereco)
            => new(customerId, productId, quantidade, precoUnitario, cep, endereco);
    }
}
