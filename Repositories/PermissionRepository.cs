using System.Data;
using Dapper;
using Rbac.Models;
using Rbac.Utils;

namespace Rbac.Repositories;

public class PermissionRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public PermissionRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = "SELECT * FROM dbo.Permissions ORDER BY Name;";

        return await conn.QueryAsync<Permission>(sql);
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT *
            FROM dbo.Permissions
            WHERE Name = @Name;
        ";

        return await conn.QueryFirstOrDefaultAsync<Permission>(sql, new { Name = name });
    }

    public async Task<Guid> CreateAsync(Permission permission)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.Permissions (Name, Description)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description);
        ";

        return await conn.ExecuteScalarAsync<Guid>(sql, permission);
    }
}
