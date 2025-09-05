using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("dev/seed-admin")]
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

        /// <summary>
        /// Cria (ou garante) um usuário ADMIN padrão para testes.
        /// email: admin@pathbit.com | senha: admin123
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Seed(CancellationToken ct)
        {
            await DbSeeder.SeedAdminAsync(_users, _customers, _uow, ct);
            return Ok(new
            {
                email = "admin@pathbit.com",
                username = "admin",
                senha = "admin123",
                dica = "Use POST /api/auth/login com usernameOrEmail=admin@pathbit.com e senha=admin123"
            });
        }
    }
}
