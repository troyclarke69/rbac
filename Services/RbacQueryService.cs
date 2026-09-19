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
}
