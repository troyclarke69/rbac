using Microsoft.AspNetCore.Mvc;
using Rbac.Middleware;
using Rbac.Repositories;
using Rbac.Models;
using Rbac.Services;

namespace Rbac.Controllers;

[ApiController]
[Route("permissions")]
public class PermissionsController : ControllerBase
{
    private readonly RbacQueryService _service;

    public PermissionsController(RbacQueryService service)
    {
        _service = service;
    }

    // -------------------------------------------------------------
    // GET /permissions
    // -------------------------------------------------------------
    [HttpGet]
    [RequirePermission("read:permissions")]
    public async Task<IActionResult> GetPermissions()
        => Ok(await _service.GetAllPermissionsAsync());

    // -------------------------------------------------------------
    // POST /permissions/create
    // -------------------------------------------------------------
    [HttpPost("create")]
    [RequirePermission("create:permissions","admin")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest req)
    {
        await _service.CreatePermissionAsync(req.Name, req.Description);
        return Ok(new { status = "permission created" });
    }

    // -------------------------------------------------------------
    // POST /permissions/update
    // -------------------------------------------------------------
    [HttpPost("update")]
    [RequirePermission("update:permissions")]
    public async Task<IActionResult> UpdatePermission([FromBody] UpdatePermissionRequest req)
    {
        await _service.UpdatePermissionAsync(req.PermissionId, req.Name, req.Description);
        return Ok(new { status = "permission updated" });
    }

    // -------------------------------------------------------------
    // POST /permissions/delete
    // -------------------------------------------------------------
    [HttpPost("delete")]
    [RequirePermission("delete:permissions")]
    public async Task<IActionResult> DeletePermission([FromBody] DeletePermissionRequest req)
    {
        await _service.DeletePermissionAsync(req.PermissionId);
        return Ok(new { status = "permission deleted" });
    }
}

// -------------------------------------------------------------
// DTOs
// -------------------------------------------------------------
public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdatePermissionRequest
{
    public Guid PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DeletePermissionRequest
{
    public Guid PermissionId { get; set; }
}
