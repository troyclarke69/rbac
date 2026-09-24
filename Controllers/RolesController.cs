using Microsoft.AspNetCore.Mvc;
using Rbac.Middleware;
using Rbac.Repositories;
using Rbac.Services;

namespace Rbac.Controllers;

[ApiController]
[Route("roles")]
public class RolesController : ControllerBase
{
    private readonly RoleRepository _roleRepo;
    private readonly RbacQueryService _rbacService;

    public RolesController(RoleRepository roleRepo, RbacQueryService rbacService)
    {
        _roleRepo = roleRepo;
        _rbacService = rbacService;
    }

    [HttpGet]
    [RequirePermission("read:roles")]
    public async Task<IActionResult> GetRoles()
    {
        return Ok(await _roleRepo.GetAllAsync());
    }
    
    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest req)
    {
        await _rbacService.AssignRoleAsync(req.UserId, req.RoleName);
        return Ok(new { status = "role assigned" });
    }

    [HttpPost("add-permission")]
    public async Task<IActionResult> AddPermissionToRole([FromBody] AddPermissionRequest req)
    {
        await _rbacService.AddPermissionToRoleAsync(req.RoleName, req.PermissionName);
        return Ok(new { status = "permission added" });
    }

    [HttpGet("role-permissions")]
    public async Task<IActionResult> GetRolePermissions()
    {
        var result = await _rbacService.GetRolePermissionsAsync();
        return Ok(result);
    }

    [HttpGet("{roleName}/detail")]
    public async Task<IActionResult> GetRoleDetail(string roleName)
    {
        var detail = await _rbacService.GetRoleDetailAsync(roleName);
        return Ok(detail);
    }

    [HttpPost("remove-permission")]
    public async Task<IActionResult> RemovePermissionFromRole([FromBody] AddPermissionRequest req)
    {
        await _rbacService.RemovePermissionFromRoleAsync(req.RoleName, req.PermissionName);
        return Ok(new { status = "permission removed" });
    }

    [HttpPost("assign-user")]
    public async Task<IActionResult> AssignUserToRole([FromBody] AssignRoleRequest req)
    {
        await _rbacService.AssignRoleAsync(req.UserId, req.RoleName);
        return Ok(new { status = "user assigned" });
    }

    [HttpPost("remove-user")]
    public async Task<IActionResult> RemoveUserFromRole([FromBody] AssignRoleRequest req)
    {
        await _rbacService.RemoveUserFromRoleAsync(req.UserId, req.RoleName);
        return Ok(new { status = "user removed" });
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest req)
    {
        await _rbacService.UpdateRoleAsync(req.RoleName, req.NewName, req.Description);
        return Ok(new { status = "role updated" });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleRequest req)
    {
        await _rbacService.DeleteRoleAsync(req.RoleName);
        return Ok(new { status = "role deleted" });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest req)
    {
        await _rbacService.CreateRoleAsync(req.RoleName, req.Description);
        return Ok(new { status = "role created" });
    }
}

public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public class AddPermissionRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
}

public class UpdateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? NewName { get; set; }
    public string? Description { get; set; }
}

public class DeleteRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
}

public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

