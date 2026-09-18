using Microsoft.AspNetCore.Mvc;
using Rbac.Middleware;
using Rbac.Repositories;
using Rbac.Models;

namespace Rbac.Controllers;

[ApiController]
[Route("permissions")]
public class PermissionsController : ControllerBase
{
    private readonly PermissionRepository _permRepo;

    public PermissionsController(PermissionRepository permRepo)
    {
        _permRepo = permRepo;
    }

    [HttpGet]
    [RequirePermission("read:permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        return Ok(await _permRepo.GetAllAsync());
    }

    public class CreatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    [HttpPost]
    [RequirePermission("write:roles")] // same permission Auth0 uses
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        var perm = new Permission
        {
            Name = request.Name,
            Description = request.Description
        };

        var id = await _permRepo.CreateAsync(perm);
        return Ok(new { id });
    }
}
