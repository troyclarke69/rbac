using Rbac.Repositories;

namespace Rbac.Services;

public class RbacQueryService
{
    private readonly RbacQueryRepository _rbacRepo;

    public RbacQueryService(RbacQueryRepository rbacRepo)
    {
        _rbacRepo = rbacRepo;
    }

    public Task AssignRoleAsync(Guid userId, string roleName)
        => _rbacRepo.AssignRoleAsync(userId, roleName);

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _rbacRepo.GetUserRolesAsync(userId);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        return await _rbacRepo.GetUserPermissionsAsync(userId);
    }

    public Task AddPermissionToRoleAsync(string roleName, string permissionName)
        => _rbacRepo.AddPermissionToRoleAsync(roleName, permissionName);

    public Task<IEnumerable<RolePermissionDto>> GetRolePermissionsAsync()
        => _rbacRepo.GetRolePermissionsAsync();

    public Task<RoleDetailDto> GetRoleDetailAsync(string roleName)
        => _rbacRepo.GetRoleDetailAsync(roleName);

    public Task RemovePermissionFromRoleAsync(string roleName, string permissionName)
        => _rbacRepo.RemovePermissionFromRoleAsync(roleName, permissionName);

    public Task RemoveUserFromRoleAsync(Guid userId, string roleName)
        => _rbacRepo.RemoveUserFromRoleAsync(userId, roleName);

    public Task UpdateRoleAsync(string roleName, string? newName, string? description)
        => _rbacRepo.UpdateRoleAsync(roleName, newName, description);

    public Task DeleteRoleAsync(string roleName)
        => _rbacRepo.DeleteRoleAsync(roleName);

    public Task CreateRoleAsync(string roleName, string? description)
        => _rbacRepo.CreateRoleAsync(roleName, description);

     public Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        => _rbacRepo.GetAllPermissionsAsync();

    public Task CreatePermissionAsync(string name, string? description)
        => _rbacRepo.CreatePermissionAsync(name, description);

    public Task UpdatePermissionAsync(Guid id, string name, string? description)
        => _rbacRepo.UpdatePermissionAsync(id, name, description);

    public Task DeletePermissionAsync(Guid id)
        => _rbacRepo.DeletePermissionAsync(id);

}
