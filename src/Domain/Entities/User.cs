namespace Domain.Entities;

public enum UserType { ADMNISTRADOR, CLIENTE }

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; //SHA-256
    public UserType Type { get; set; } = UserType.CLIENTE;

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
