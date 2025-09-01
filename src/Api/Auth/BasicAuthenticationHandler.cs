using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;                 // UrlEncoder
using Application.Interfaces;
using Common.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Auth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Basic";

    private readonly IUserRepository _users;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,                       // ✅ use ISystemClock
        IUserRepository users)
        : base(options, logger, encoder, clock)   // ✅ compatível
    {
        _users = users;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid Scheme");

        string token;
        try { token = authHeader["Basic ".Length..].Trim(); }
        catch { return AuthenticateResult.Fail("Invalid Authorization Header"); }

        byte[] credentialBytes;
        try { credentialBytes = Convert.FromBase64String(token); }
        catch { return AuthenticateResult.Fail("Invalid Base64"); }

        var parts = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        if (parts.Length != 2) return AuthenticateResult.Fail("Invalid Basic credentials");

        var username = parts[0];
        var password = parts[1];

        var user = await _users.GetByUserNameOrEmailAsync(username);
        if (user is null) return AuthenticateResult.Fail("User not found");

        var hash = PasswordHasher.Sha256(password);
        if (!hash.Equals(user.PasswordHash, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid password");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Type.ToString()),
            new Claim("customerId", user.CustomerId?.ToString() ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
