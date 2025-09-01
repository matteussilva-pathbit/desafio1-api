namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "ENVIADO";

    // FK
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    // Entrega
    public string Cep { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
