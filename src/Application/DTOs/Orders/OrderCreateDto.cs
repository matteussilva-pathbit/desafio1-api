namespace Application.DTOs.Orders;

public class OrderCreateDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Cep { get; set; } = "";
}
