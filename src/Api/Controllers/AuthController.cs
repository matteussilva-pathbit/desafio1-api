using System.Threading;
using System.Threading.Tasks;
using Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        public sealed class SignUpRequest
        {
            public string Nome { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Username { get; set; } = default!;
            public string Senha { get; set; } = default!;
        }

        public sealed class LoginRequest
        {
            public string UsernameOrEmail { get; set; } = default!;
            public string Senha { get; set; } = default!;
        }

        [HttpPost("signup")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest req, CancellationToken ct)
        {
            var token = await _auth.SignUpAsync(req.Nome, req.Email, req.Username, req.Senha, ct);
            return Ok(new { token });
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var token = await _auth.LoginAsync(req.UsernameOrEmail, req.Senha, ct);
            return Ok(new { token });
        }
    }
}
