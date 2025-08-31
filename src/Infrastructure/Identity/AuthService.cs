using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs.Auth;
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<AppUser> userManager,
                       SignInManager<AppUser> signInManager,
                       IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var user = new AppUser { Email = dto.Email, UserName = dto.UserName };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return false;

        // garante role enviada
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        return true;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByNameAsync(dto.UserNameOrEmail)
                   ?? await _userManager.FindByEmailAsync(dto.UserNameOrEmail);
        if (user is null) return null;

        var pwdOk = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!pwdOk.Succeeded) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        var keyStr = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyStr)) return null;

        var key = Encoding.UTF8.GetBytes(keyStr);
        var expires = DateTime.UtcNow.AddHours(2);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Role, role)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });

        return new AuthResponseDto
        {
            AccessToken = tokenHandler.WriteToken(token),
            UserName = user.UserName ?? string.Empty,
            Role = role,
            ExpiresAt = expires
        };
    }
}
