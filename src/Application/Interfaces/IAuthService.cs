using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default);
}
