namespace Application.Interfaces;

public interface IOrderService
{
    Task<Domain.Entities.Order> CreateAsync(Guid customerId, Guid productId, int quantity, string cep, CancellationToken ct = default);
}
