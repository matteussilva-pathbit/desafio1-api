using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken ct);
    }
}
