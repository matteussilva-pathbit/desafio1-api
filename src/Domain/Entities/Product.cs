using System;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Nome { get; private set; } = default!;
        public decimal Preco { get; private set; }

        // Use inglês para alinhar com a camada Application
        public int QuantityAvailable { get; private set; }

        private Product() { }

        private Product(string nome, decimal preco, int quantityAvailable)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome é obrigatório.");
            if (preco <= 0) throw new DomainException("Preço deve ser maior que zero.");
            if (quantityAvailable < 0) throw new DomainException("Quantidade disponível não pode ser negativa.");

            Nome = nome.Trim();
            Preco = preco;
            QuantityAvailable = quantityAvailable;
        }

        public static Product Create(string nome, decimal preco, int quantityAvailable) =>
            new(nome, preco, quantityAvailable);

        public void DebitarEstoque(int quantidade)
        {
            if (quantidade <= 0) throw new DomainException("Quantidade deve ser > 0.");
            if (QuantityAvailable < quantidade) throw new DomainException("Estoque insuficiente.");
            QuantityAvailable -= quantidade;
        }
    }
}
