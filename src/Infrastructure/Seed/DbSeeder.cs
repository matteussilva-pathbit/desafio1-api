using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IUserRepository users, ICustomerRepository customers, IUnitOfWork uow, CancellationToken ct = default)
        {
            // evita admin duplicado
            var existing = await users.GetByEmailAsync("admin@pathbit.com", ct);
            if (existing is not null) return;

            await uow.BeginTransactionAsync(ct);
            try
            {
                // cria um Customer para vincular se quiser (opcional para ADMIN)
                var customer = Customer.Create("Administrador", "admin@pathbit.com");
                await customers.AddAsync(customer, ct);

                // hash fixo para senha "admin123"
                var hash = SHA256("admin123");

                // ⚠️ nossa entidade User espera string para Type
                var admin = User.Create(
                    email: "admin@pathbit.com",
                    userName: "admin",
                    passwordHash: hash,
                    type: "ADMIN",
                    customerId: customer.Id);

                await users.AddAsync(admin, ct);

                await uow.SaveChangesAsync(ct);
                await uow.CommitAsync(ct);
            }
            catch
            {
                await uow.RollbackAsync(ct);
                throw;
            }
        }

        private static string SHA256(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            var sb = new System.Text.StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
