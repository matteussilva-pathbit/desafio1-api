using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface;
using Domain;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity
{
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository users, ICustomerRepository customers, IUnitOfWork uow, IConfiguration config)
        {
            _users = users;
            _customers = customers;
            _uow = uow;
            _config = config;
        }

        public async Task<string> SignUpAsync(string nome, string email, string username, string senha, CancellationToken ct)
        {
            // hash SHA256
            var hash = ComputeSha256(senha);

            var customer = Customer.Create(nome, email);
            var user = User.Create(email, username, hash, UserType.Cliente);

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

            // token JWT com 1h
            return GenerateJwt(customer.Id, customer.Nome, customer.Email, UserType.Cliente);
        }

        public async Task<string> LoginAsync(string usernameOrEmail, string senha, CancellationToken ct)
        {
            var hash = ComputeSha256(senha);

            // Tenta por email; se quiser, adicione GetByUsernameAsync no repo
            var user = await _users.GetByEmailAsync(usernameOrEmail, ct);
            if (user is null)
                throw new DomainException("Usuário não encontrado.");

            if (!string.Equals(user.PasswordHash, hash, StringComparison.OrdinalIgnoreCase))
                throw new DomainException("Credenciais inválidas.");

            // Neste modelo simples, fazemos lookup do customer pelo e-mail do user
            var customer = await _customers.GetByEmailAsync(user.Email, ct);
            if (customer is null)
                throw new DomainException("Cliente não encontrado para o usuário.");

            return GenerateJwt(customer.Id, customer.Nome, customer.Email, user.Tipo);
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        private string GenerateJwt(Guid customerId, string nome, string email, UserType tipo)
        {
            var secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret não configurado.");
            var issuer = _config["Jwt:Issuer"] ?? "desafio-api";
            var audience = _config["Jwt:Audience"] ?? "desafio-api-clients";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, customerId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("name", nome),
                new Claim(ClaimTypes.Role, tipo == UserType.Administrador ? "ADMIN" : "CLIENTE")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1), // 1 hora
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
