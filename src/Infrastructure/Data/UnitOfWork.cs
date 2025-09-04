using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _ctx;
        private IDbContextTransaction? _tx;

        public UnitOfWork(AppDbContext ctx) => _ctx = ctx;

        public async Task BeginTransactionAsync(CancellationToken ct)
            => _tx = await _ctx.Database.BeginTransactionAsync(ct);

        public Task SaveChangesAsync(CancellationToken ct)
            => _ctx.SaveChangesAsync(ct);

        public async Task CommitAsync(CancellationToken ct)
        {
            if (_tx is not null)
            {
                await _tx.CommitAsync(ct);
                await _tx.DisposeAsync();
                _tx = null;
            }
        }

        public async Task RollbackAsync(CancellationToken ct)
        {
            if (_tx is not null)
            {
                await _tx.RollbackAsync(ct);
                await _tx.DisposeAsync();
                _tx = null;
            }
        }
    }
}
