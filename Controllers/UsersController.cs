using Microsoft.AspNetCore.Mvc;
using Rbac.Middleware;
using Rbac.Repositories;
using Rbac.Services;
using Rbac.Models;

namespace Rbac.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UserRepository _userRepo;
    private readonly RoleRepository _roleRepo;
    private readonly AuthService _authService;
    private readonly RbacQueryService _rbacService;
    private readonly JwtIssuer _jwtIssuer;

    public UsersController(
        UserRepository userRepo, 
        RoleRepository roleRepo, 
        AuthService authService,
        RbacQueryService rbacService,
        JwtIssuer jwtIssuer)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _authService = authService;
        _rbacService = rbacService;
        _jwtIssuer = jwtIssuer;
    }

    // Expects 403: Forbidden as admin account is created without role
    [HttpPost("bootstrap-admin")]
    public async Task<IActionResult> BootstrapAdmin()
    {
        var id = await _authService.RegisterUserAsync("admin@example.com", "Swed9999!");
        return Ok(new { id });
    }

    [HttpPost("bootstrap-admin-with-role")]
    public async Task<IActionResult> BootstrapAdminWithRole()
    {
        var id = await _authService.RegisterUserAsync(
            "superadmin@example.com",
            "Swed9999!"
        );

        await _rbacService.AssignRoleAsync(id, "manager");

        var token = await _jwtIssuer.GenerateTokenAsync(id);

        return Ok(new { id, token });
    }

    [HttpGet]
    [RequirePermission("read:users")]
    public async Task<IActionResult> GetUsers()
    {
        // For demo purposes, return all users
        // (You can add paging later)
        return Ok(await _userRepo.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("read:users")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost]
    [RequirePermission("write:users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var id = await _authService.RegisterUserAsync(request.Email, request.Password);
        return Ok(new { id });
    }
}
