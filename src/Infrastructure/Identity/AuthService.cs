using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interface;
using Common;
using Domain.Entities;
using Domain.Enums; // se existir o enum UserType
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity
{
    public sealed class AuthService : IAuthService
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _config;
        private readonly IUserRepository _users;
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _uow;

        public AuthService(
            AppDbContext ctx,
            IConfiguration config,
            IUserRepository users,
            ICustomerRepository customers,
            IUnitOfWork uow)
        {
            _ctx = ctx;
            _config = config;
            _users = users;
            _customers = customers;
            _uow = uow;
        }

        public async Task<string> SignUpAsync(string nome, string email, string username, string senha, CancellationToken ct)
        {
            // cria Customer + User (CLIENTE)
            var hash = Sha256(senha);

            await _uow.BeginTransactionAsync(ct);
            try
            {
                var customer = Customer.Create(nome, email);
                await _customers.AddAsync(customer, ct);

                // ⚠️ nossa entidade User espera string para Type
                var typeStr = "CLIENTE";

                var user = User.Create(
                    email: email,
                    userName: username,
                    passwordHash: hash,
                    type: typeStr,
                    customerId: customer.Id);

                await _users.AddAsync(user, ct);

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);

                return IssueJwt(user, customer);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<string> LoginAsync(string usernameOrEmail, string senha, CancellationToken ct)
        {
            var hash = Sha256(senha);

            var user = await _ctx.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    (u.Email == usernameOrEmail || u.UserName == usernameOrEmail) &&
                    u.PasswordHash == hash, ct);

            if (user is null)
                throw new DomainException("Credenciais inválidas.");

            // carrega customer se houver
            Customer? customer = null;
            if (user.CustomerId.HasValue)
                customer = await _ctx.Customers.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == user.CustomerId.Value, ct);

            return IssueJwt(user, customer);
        }

        private string IssueJwt(User user, Customer? customer)
        {
            var secret = _config["Jwt:Secret"] ?? "dev-secret-change-me-please";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.UserName),
                // ⚠️ role precisa ser string
                new Claim(ClaimTypes.Role, user.Type)
            };

            if (customer is not null)
            {
                claims.Add(new Claim("customerId", customer.Id.ToString()));
                claims.Add(new Claim("customerName", customer.Nome));
            }

            var token = new JwtSecurityToken(
                issuer: "api",
                audience: "api-clients",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string Sha256(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
