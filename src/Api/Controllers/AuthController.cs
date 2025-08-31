using Application.DTOs.Auth;
using Application.Interfaces;
using Infrastructure.Identity;                 // <- AQUI!
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly UserManager<AppUser> _userManager;           // <- AppUser de Infrastructure.Identity
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(IAuthService auth,
                          UserManager<AppUser> userManager,       // <- SEM "Domain.Entities."
                          RoleManager<IdentityRole> roleManager)
    {
        _auth = auth;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ... resto do controller
}
