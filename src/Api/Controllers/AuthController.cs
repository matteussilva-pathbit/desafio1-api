using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("signup")]
    [AllowAnonymous] // sem JWT
    public async Task<IActionResult> SignUp([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var ok = await _auth.RegisterAsync(dto, ct);
        return ok ? Ok() : BadRequest("Não foi possível registrar.");
    }

    [HttpPost("login")]
    [AllowAnonymous] // sem JWT
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(dto, ct);
        return result is null ? Unauthorized() : Ok(result);
    }
}
