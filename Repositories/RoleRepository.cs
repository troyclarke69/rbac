using System.Data;
using Dapper;
using Rbac.Models;
using Rbac.Utils;

namespace Rbac.Repositories;

public class RoleRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public RoleRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = "SELECT * FROM dbo.Roles ORDER BY Name;";

        return await conn.QueryAsync<Role>(sql);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT *
            FROM dbo.Roles
            WHERE Name = @Name;
        ";

        return await conn.QueryFirstOrDefaultAsync<Role>(sql, new { Name = name });
    }

    public async Task<Guid> CreateAsync(Role role)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.Roles (Name, Description)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description);
        ";

        return await conn.ExecuteScalarAsync<Guid>(sql, role);
    }
}
