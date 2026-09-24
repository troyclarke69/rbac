using System.Data;
using Dapper;
using Rbac.Utils;
using Rbac.Models;

namespace Rbac.Repositories;

public class RbacQueryRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public RbacQueryRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // -------------------------------------------------------------
    // Assign Role
    // -------------------------------------------------------------
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

    // -------------------------------------------------------------
    // Get User Roles
    // -------------------------------------------------------------
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

    // -------------------------------------------------------------
    // Get User Permissions
    // -------------------------------------------------------------
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

    // -------------------------------------------------------------
    // Add Permission to Role
    // -------------------------------------------------------------
    public async Task AddPermissionToRoleAsync(string roleName, string permissionName)
    {
        using var conn = _connectionFactory.CreateConnection();

        var roleId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT Id FROM dbo.Roles WHERE Name = @name",
            new { name = roleName }
        );

        var permId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT Id FROM dbo.Permissions WHERE Name = @name",
            new { name = permissionName }
        );

        await conn.ExecuteAsync(
            @"INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
              VALUES (@roleId, @permId)",
            new { roleId, permId }
        );
    }

    // -------------------------------------------------------------
    // Get Role Permissions Matrix
    // -------------------------------------------------------------
    public async Task<IEnumerable<RolePermissionDto>> GetRolePermissionsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();

        return await conn.QueryAsync<RolePermissionDto>(
            @"SELECT 
                rp.RoleId,
                r.Name AS RoleName,
                rp.PermissionId,
                p.Name AS PermissionName
            FROM dbo.RolePermissions rp
            INNER JOIN dbo.Roles r ON rp.RoleId = r.Id
            INNER JOIN dbo.Permissions p ON rp.PermissionId = p.Id"
        );
    }

    // -------------------------------------------------------------
    // Get Role Detail (with full user objects)
    // -------------------------------------------------------------
    public async Task<RoleDetailDto> GetRoleDetailAsync(string roleName)
    {
        using var conn = _connectionFactory.CreateConnection();

        var role = await conn.QuerySingleAsync<Role>(
            "SELECT Id, Name, Description FROM dbo.Roles WHERE Name = @name",
            new { name = roleName }
        );

        var permissions = await conn.QueryAsync<string>(
            @"SELECT p.Name
              FROM dbo.RolePermissions rp
              INNER JOIN dbo.Permissions p ON rp.PermissionId = p.Id
              WHERE rp.RoleId = @roleId",
            new { roleId = role.Id }
        );

        var users = await conn.QueryAsync<UserDto>(
            @"SELECT u.Id, u.Email
              FROM dbo.UserRoles ur
              INNER JOIN dbo.Users u ON ur.UserId = u.Id
              WHERE ur.RoleId = @roleId",
            new { roleId = role.Id }
        );

        return new RoleDetailDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            Description = role.Description,
            Permissions = permissions.ToList(),
            Users = users.ToList()
        };
    }

    // -------------------------------------------------------------
    // Remove Permission from Role
    // -------------------------------------------------------------
    public async Task RemovePermissionFromRoleAsync(string roleName, string permissionName)
    {
        using var conn = _connectionFactory.CreateConnection();

        var roleId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT Id FROM dbo.Roles WHERE Name = @name",
            new { name = roleName }
        );

        var permId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT Id FROM dbo.Permissions WHERE Name = @name",
            new { name = permissionName }
        );

        await conn.ExecuteAsync(
            @"DELETE FROM dbo.RolePermissions 
              WHERE RoleId = @roleId AND PermissionId = @permId",
            new { roleId, permId }
        );
    }

    // -------------------------------------------------------------
    // Remove User from Role
    // -------------------------------------------------------------
    public async Task RemoveUserFromRoleAsync(Guid userId, string roleName)
    {
        using var conn = _connectionFactory.CreateConnection();

        var roleId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT Id FROM dbo.Roles WHERE Name = @name",
            new { name = roleName }
        );

        await conn.ExecuteAsync(
            @"DELETE FROM dbo.UserRoles 
              WHERE UserId = @userId AND RoleId = @roleId",
            new { userId, roleId }
        );
    }

    // -------------------------------------------------------------
    // Update Role
    // -------------------------------------------------------------
    public async Task UpdateRoleAsync(string roleName, string? newName, string? description)
    {
        using var conn = _connectionFactory.CreateConnection();

        await conn.ExecuteAsync(
            @"UPDATE dbo.Roles
              SET Name = COALESCE(@newName, Name),
                  Description = COALESCE(@description, Description)
              WHERE Name = @roleName",
            new { roleName, newName, description }
        );
    }

    // -------------------------------------------------------------
    // Delete Role
    // -------------------------------------------------------------
    public async Task DeleteRoleAsync(string roleName)
    {
        using var conn = _connectionFactory.CreateConnection();

        var roleId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT Id FROM dbo.Roles WHERE Name = @name",
            new { name = roleName }
        );

        await conn.ExecuteAsync(
            @"DELETE FROM dbo.UserRoles WHERE RoleId = @roleId",
            new { roleId }
        );

        await conn.ExecuteAsync(
            @"DELETE FROM dbo.RolePermissions WHERE RoleId = @roleId",
            new { roleId }
        );

        await conn.ExecuteAsync(
            @"DELETE FROM dbo.Roles WHERE Id = @roleId",
            new { roleId }
        );
    }

    // -------------------------------------------------------------
    // Create Role
    // -------------------------------------------------------------
    public async Task CreateRoleAsync(string roleName, string? description)
    {
        using var conn = _connectionFactory.CreateConnection();

        await conn.ExecuteAsync(
            @"INSERT INTO dbo.Roles (Id, Name, Description)
              VALUES (NEWID(), @name, @description)",
            new { name = roleName, description }
        );
    }

    // -------------------------------------------------------------
    // PERMISSIONS CRUD
    // -------------------------------------------------------------

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();

        return await conn.QueryAsync<PermissionDto>(
            @"SELECT 
                p.Id,
                p.Name,
                p.Description,
                (SELECT STRING_AGG(r.Name, ', ')
                 FROM dbo.RolePermissions rp
                 INNER JOIN dbo.Roles r ON rp.RoleId = r.Id
                 WHERE rp.PermissionId = p.Id) AS Roles
              FROM dbo.Permissions p"
        );
    }

    public async Task CreatePermissionAsync(string name, string? description)
    {
        using var conn = _connectionFactory.CreateConnection();

        await conn.ExecuteAsync(
            @"INSERT INTO dbo.Permissions (Id, Name, Description)
              VALUES (NEWID(), @name, @description)",
            new { name, description }
        );
    }

    public async Task UpdatePermissionAsync(Guid id, string name, string? description)
    {
        using var conn = _connectionFactory.CreateConnection();

        await conn.ExecuteAsync(
            @"UPDATE dbo.Permissions
              SET Name = @name,
                  Description = @description
              WHERE Id = @id",
            new { id, name, description }
        );
    }

    public async Task DeletePermissionAsync(Guid id)
    {
        using var conn = _connectionFactory.CreateConnection();

        await conn.ExecuteAsync(
            @"DELETE FROM dbo.RolePermissions WHERE PermissionId = @id;
              DELETE FROM dbo.Permissions WHERE Id = @id;",
            new { id }
        );
    }
}

// -------------------------------------------------------------
// DTOs
// -------------------------------------------------------------
public class RolePermissionDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
}

public class RoleDetailDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<string> Permissions { get; set; } = new();
    public List<UserDto> Users { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Roles { get; set; }
}
