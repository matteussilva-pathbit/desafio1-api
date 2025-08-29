namespace Application.DTOs.Product;

public class ProductUpdateDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set;  }
}