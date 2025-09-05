using System;
using Common;

namespace Domain.Entities
{
    public sealed class Product
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Nome { get; private set; } = default!;
        public decimal Preco { get; private set; }
        public int QuantityAvailable { get; private set; }

        private Product() { } // EF

        private Product(string nome, decimal preco, int quantityAvailable)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome inválido.");

            if (preco <= 0)
                throw new DomainException("Preço deve ser maior que zero.");

            if (quantityAvailable < 0)
                throw new DomainException("Estoque inicial não pode ser negativo.");

            Nome = nome.Trim();
            Preco = preco;
            QuantityAvailable = quantityAvailable;
        }

        public static Product Create(string nome, decimal preco, int quantityAvailable)
            => new Product(nome, preco, quantityAvailable);

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new DomainException("Preço deve ser maior que zero.");
            Preco = newPrice;
        }

        public void IncreaseInventory(int delta)
        {
            if (delta <= 0)
                throw new DomainException("Delta de entrada deve ser > 0.");
            checked
            {
                QuantityAvailable += delta;
            }
        }

        public void DebitInventory(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantidade para débito deve ser > 0.");

            if (QuantityAvailable < quantity)
                throw new DomainException("Estoque insuficiente.");

            QuantityAvailable -= quantity;
        }
    }
}
