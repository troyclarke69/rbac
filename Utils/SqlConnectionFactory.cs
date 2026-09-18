using System.Data;
using Microsoft.Data.SqlClient;

namespace Rbac.Utils;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("AuthDemo")
            ?? throw new InvalidOperationException("Connection string 'AuthDemo' not found.");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
