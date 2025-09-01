using Application.DTOs.Orders;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<Guid?> CreateAsync(OrderCreateDto dto, Guid customerId, CancellationToken ct = default);
}
