using System;

namespace Domain.Entities
{
    public enum UserType
    {
        CLIENTE,
        ADMINISTRADOR
    }

    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public UserType Tipo { get; private set; }

        public User(string username, string email, string password, UserType tipo)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            Password = password;
            Tipo = tipo;
        }
    }
}
