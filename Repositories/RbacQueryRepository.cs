using System.Data;
using Dapper;
using Rbac.Utils;

namespace Rbac.Repositories;

public class RbacQueryRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public RbacQueryRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AssignRoleAsync(Guid userId, string roleName)
    {
        using var conn = _connectionFactory.CreateConnection();

        var roleId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT Id FROM dbo.Roles WHERE Name = @name",
            new { name = roleName });

        await conn.ExecuteAsync(
            @"INSERT INTO dbo.UserRoles (UserId, RoleId)
              VALUES (@userId, @roleId)",
            new { userId, roleId });
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT r.Name
            FROM dbo.UserRoles ur
            JOIN dbo.Roles r ON ur.RoleId = r.Id
            WHERE ur.UserId = @UserId;
        ";

        var roles = await conn.QueryAsync<string>(sql, new { UserId = userId });
        return roles.ToList();
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT p.Name
            FROM dbo.UserRoles ur
            JOIN dbo.RolePermissions rp ON ur.RoleId = rp.RoleId
            JOIN dbo.Permissions p ON rp.PermissionId = p.Id
            WHERE ur.UserId = @UserId;
        ";

        var permissions = await conn.QueryAsync<string>(sql, new { UserId = userId });
        return permissions.ToList();
    }
}
