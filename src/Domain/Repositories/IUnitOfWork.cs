using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        Task CommitAsync(CancellationToken ct);
        Task RollbackAsync(CancellationToken ct);
    }
}
