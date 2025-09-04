using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Nome { get; private set; } = default!;
        public string Email { get; private set; } = default!;

        public ICollection<Order> Orders { get; private set; } = new List<Order>();

        private Customer() { }

        private Customer(string nome, string email)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email é obrigatório.");

            Nome = nome.Trim();
            Email = email.Trim();
        }

        public static Customer Create(string nome, string email) => new(nome, email);
    }
}
