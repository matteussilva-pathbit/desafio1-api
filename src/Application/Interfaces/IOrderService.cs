using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IOrderService
    {
        Task<Guid> CreateAsync(Guid customerId, Guid productId, int quantity, string cep, CancellationToken ct = default);
    }
}
