using System;
using Common;

namespace Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Email { get; private set; } = default!;
        public string UserName { get; private set; } = default!;     // <- usado no AppDbContext
        public string PasswordHash { get; private set; } = default!;
        public string Type { get; private set; } = default!;         // <- "ADMIN" | "CLIENTE" (usado no AppDbContext)
        public Guid? CustomerId { get; private set; }                // cliente vinculado quando Type = CLIENTE

        private User() { } // EF

        private User(string email, string userName, string passwordHash, string type, Guid? customerId)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email é obrigatório.");
            if (string.IsNullOrWhiteSpace(userName)) throw new DomainException("UserName é obrigatório.");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("PasswordHash é obrigatório.");
            if (string.IsNullOrWhiteSpace(type)) throw new DomainException("Type é obrigatório.");

            Email = email.Trim();
            UserName = userName.Trim();
            PasswordHash = passwordHash;
            Type = type.Trim().ToUpperInvariant();
            CustomerId = customerId;
        }

        public static User Create(string email, string userName, string passwordHash, string type, Guid? customerId = null)
            => new User(email, userName, passwordHash, type, customerId);

        public void SetPasswordHash(string newHash)
        {
            if (string.IsNullOrWhiteSpace(newHash)) throw new DomainException("Hash inválido.");
            PasswordHash = newHash;
        }

        public void SetType(string newType)
        {
            if (string.IsNullOrWhiteSpace(newType)) throw new DomainException("Tipo inválido.");
            Type = newType.Trim().ToUpperInvariant();
        }
    }
}
