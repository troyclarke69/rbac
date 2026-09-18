using Microsoft.AspNetCore.Mvc;
using Rbac.Middleware;
using Rbac.Repositories;
using Rbac.Models;

namespace Rbac.Controllers;

[ApiController]
[Route("roles")]
public class RolesController : ControllerBase
{
    private readonly RoleRepository _roleRepo;
    private readonly PermissionRepository _permRepo;

    public RolesController(RoleRepository roleRepo, PermissionRepository permRepo)
    {
        _roleRepo = roleRepo;
        _permRepo = permRepo;
    }

    [HttpGet]
    [RequirePermission("read:roles")]
    public async Task<IActionResult> GetRoles()
    {
        return Ok(await _roleRepo.GetAllAsync());
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    [HttpPost]
    [RequirePermission("write:roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        var id = await _roleRepo.CreateAsync(role);
        return Ok(new { id });
    }
}
