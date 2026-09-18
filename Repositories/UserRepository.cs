using System.Data;
using Dapper;
using Rbac.Models;
using Rbac.Utils;

namespace Rbac.Repositories;

public class UserRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public UserRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = "SELECT * FROM dbo.Users ORDER BY Email;";

        return await conn.QueryAsync<User>(sql);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT *
            FROM dbo.Users
            WHERE Email = @Email;
        ";

        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT *
            FROM dbo.Users
            WHERE Id = @UserId;
        ";

        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<Guid> CreateAsync(User user)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.Users (Email, PasswordHash, PasswordSalt, IsEmailVerified, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Email, @PasswordHash, @PasswordSalt, @IsEmailVerified, @IsActive);
        ";

        return await conn.ExecuteScalarAsync<Guid>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.Users
            SET Email = @Email,
                PasswordHash = @PasswordHash,
                PasswordSalt = @PasswordSalt,
                IsEmailVerified = @IsEmailVerified,
                IsActive = @IsActive,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
        ";

        await conn.ExecuteAsync(sql, user);
    }
}
