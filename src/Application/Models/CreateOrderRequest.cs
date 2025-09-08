namespace Application.Models
{
    public sealed class CreateOrderRequest
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
        public string Cep { get; init; } = string.Empty;
        public string? AddressOverride { get; init; }
    }
}
