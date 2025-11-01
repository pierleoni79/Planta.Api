// Ruta: /Planta.Data/Abstractions/ISqlConnectionFactory.cs | V1.0
#nullable enable
using System.Data;
using Microsoft.Data.SqlClient;

namespace Planta.Data.Abstractions;

/// <summary>Fábrica simple para abrir conexiones SQL.</summary>
public interface ISqlConnectionFactory
{
    SqlConnection Create();
    Task<SqlConnection> CreateOpenAsync(CancellationToken ct = default);
}
