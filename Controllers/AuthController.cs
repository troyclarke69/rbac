using Microsoft.AspNetCore.Mvc;
using Rbac.Services;
using Rbac.Models;

namespace Rbac.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtIssuer _jwtIssuer;

    public AuthController(AuthService authService, JwtIssuer jwtIssuer)
    {
        _authService = authService;
        _jwtIssuer = jwtIssuer;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.ValidateCredentialsAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new { error = "Invalid credentials" });

        var token = await _jwtIssuer.GenerateTokenAsync(user.Id);

        return Ok(new { access_token = token });
    }
}
