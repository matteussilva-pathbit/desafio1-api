using System;

namespace Domain.Entities
{
    public enum OrderStatus
    {
        ENVIADO
    }

    public class Order
    {
        public Guid Id { get; private set; }
        public DateTime DataPedido { get; private set; }
        public OrderStatus Status { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantidade { get; private set; }
        public decimal Preco { get; private set; }
        public string CepEntrega { get; private set; }
        public string EnderecoEntrega { get; private set; }

        public Order(Guid customerId, Guid productId, int quantidade, decimal preco, string cepEntrega, string enderecoEntrega)
        {
            Id = Guid.NewGuid();
            DataPedido = DateTime.UtcNow;
            Status = OrderStatus.ENVIADO;
            CustomerId = customerId;
            ProductId = productId;
            Quantidade = quantidade;
            Preco = preco;
            CepEntrega = cepEntrega;
            EnderecoEntrega = enderecoEntrega;
        }
    }
}
