using System.IdentityModel.Tokens.Jwt;   // correto
using System.Security.Claims;
using System.Text;
using Application.DTOs.Auth;
using Application.Interfaces;
using Common.Security;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly AppDbContext _ctx;
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext ctx, IUserRepository users, IConfiguration config)
    {
        _ctx = ctx;
        _users = users;
        _config = config;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        // cria Customer
        var customer = new Customer { Name = dto.Name, Email = dto.Email };
        _ctx.Customers.Add(customer);
        await _ctx.SaveChangesAsync(ct);

        // cria User com hash SHA-256
        var typeOk = Enum.TryParse<UserType>(dto.Type, true, out var userType);
        var user = new User
        {
            Email = dto.Email,
            UserName = dto.UserName,
            PasswordHash = PasswordHasher.Sha256(dto.Password),
            Type = typeOk ? userType : UserType.CLIENTE,
            CustomerId = customer.Id
        };

        _ctx.Users.Add(user);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByUserNameOrEmailAsync(dto.UserNameOrEmail, ct);
        if (user is null) return null;

        var hash = PasswordHasher.Sha256(dto.Password);
        if (!hash.Equals(user.PasswordHash, StringComparison.OrdinalIgnoreCase)) return null;

        var expires = DateTime.UtcNow.AddHours(1);
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        var claims = new List<Claim>
        {
            new("uid", user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Type.ToString()),
            new("customerId", user.CustomerId?.ToString() ?? "")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return new AuthResponseDto
        {
            AccessToken = handler.WriteToken(token),
            ExpiresAt = expires,
            UserName = user.UserName,
            Role = user.Type.ToString(),
            CustomerId = user.CustomerId,
            Email = user.Email,
            Name = user.Customer?.Name ?? ""
        };
    }
}
