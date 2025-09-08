// src/API/Auth/BasicAuthenticationHandler.cs
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Common.Security;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Auth;

public sealed class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppDbContext _ctx;

     public BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AppDbContext ctx)
    : base(options, logger, encoder)
{
    _ctx = ctx;
}


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Sem header → deixa outros esquemas (JWT) cuidarem
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        AuthenticationHeaderValue authHeader;
        try
        {
            var raw = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(raw)) return AuthenticateResult.NoResult();

            authHeader = AuthenticationHeaderValue.Parse(raw);
        }
        catch
        {
            return AuthenticateResult.Fail("Authorization header inválido.");
        }

        if (!"Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        // decodifica credenciais
        string usernameOrEmail, senha;
        try
        {
            var parameter = authHeader.Parameter ?? "";
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(parameter)).Split(':', 2);
            if (credentials.Length != 2) return AuthenticateResult.Fail("Credenciais inválidas.");
            usernameOrEmail = credentials[0];
            senha = credentials[1];
        }
        catch
        {
            return AuthenticateResult.Fail("Credenciais em Base64 inválidas.");
        }

        // Hash SHA256 (hex minúsculo) — usa o helper comum
        var senhaHash = PasswordHasher.Hash(senha);

        // Busca por Email OU UserName
        var user = await _ctx.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == usernameOrEmail || u.UserName == usernameOrEmail);

        if (user is null || !string.Equals(user.PasswordHash, senhaHash, StringComparison.Ordinal))
            return AuthenticateResult.Fail("Usuário ou senha inválidos.");

        // Monta identidade
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Type) // "ADMIN" | "CLIENTE"
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
