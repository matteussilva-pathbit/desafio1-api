using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        // Regra [1]: GUID gerado automaticamente
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Email { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public UserType Tipo { get; private set; }

        // EF
        private User() { }

        private User(string email, string username, string passwordHash, UserType tipo)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email é obrigatório.");
            if (string.IsNullOrWhiteSpace(username)) throw new DomainException("Username é obrigatório.");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("PasswordHash é obrigatório.");

            Email = email.Trim();
            Username = username.Trim();
            PasswordHash = passwordHash;
            Tipo = tipo;
        }

        public static User Create(string email, string username, string passwordHash, UserType tipo)
            => new(email, username, passwordHash, tipo);
    }
}
