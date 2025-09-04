using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;

namespace Infrastructure.Seed
{
    public sealed class DbSeeder
    {
        private readonly IUserRepository _users;
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _uow;

        public DbSeeder(IUserRepository users, ICustomerRepository customers, IUnitOfWork uow)
        {
            _users = users;
            _customers = customers;
            _uow = uow;
        }

        public async Task SeedAsync(CancellationToken ct)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes("admin123"));
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            var customer = Customer.Create("Administrador", "admin@pathbit.com");
            var user = User.Create("admin@pathbit.com", "admin", hash, UserType.Administrador);

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _customers.AddAsync(customer, ct);
                await _users.AddAsync(user, ct);

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }
    }
}
