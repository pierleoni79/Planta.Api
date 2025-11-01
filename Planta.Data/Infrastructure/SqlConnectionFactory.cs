// Ruta: /Planta.Data/Infrastructure/SqlConnectionFactory.cs | V1.0
#nullable enable
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Planta.Data.Abstractions;

namespace Planta.Data.Infrastructure;

public sealed class SqlConnectionFactory(IConfiguration config) : ISqlConnectionFactory
{
    private readonly string _cnn = config.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("ConnectionString 'SqlServer' no configurada.");

    public SqlConnection Create() => new(_cnn);

    public async Task<SqlConnection> CreateOpenAsync(CancellationToken ct = default)
    {
        var cn = new SqlConnection(_cnn);
        await cn.OpenAsync(ct);
        return cn;
    }
}
