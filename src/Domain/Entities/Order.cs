using Domain.Enums;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.ENVIADO;

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string DeliveryZipCode { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public List<OrderItem> Items { get; set; } = new();
}
