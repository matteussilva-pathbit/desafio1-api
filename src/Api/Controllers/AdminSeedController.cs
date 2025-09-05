using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain.Enums;
using Domain.Repositories;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("dev/seed-admin")] // rota temporária de dev; remova em produção
    public sealed class AdminSeedController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _uow;

        public AdminSeedController(IUserRepository users, ICustomerRepository customers, IUnitOfWork uow)
        {
            _users = users;
            _customers = customers;
            _uow = uow;
        }

        [HttpPost]
        public async Task<IActionResult> Seed(CancellationToken ct)
        {
            var email = "admin@pathbit.com";
            var username = "admin";
            var senha = "admin123";

            // Sem Create(); usa método estático para evitar ambiguidade
            var hashBytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(senha));
            var hash = System.Convert.ToHexString(hashBytes).ToLowerInvariant();

            // ⚠️ Use os tipos de domínio totalmente qualificados para não colidir com ControllerBase.User
            var customer = Domain.Entities.Customer.Create("Administrador", email);
            var user = Domain.Entities.User.Create(email, username, hash, UserType.Administrador);

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

            return Ok(new { email, username, senha });
        }
    }
}
