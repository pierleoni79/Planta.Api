// Ruta: /Planta.Infrastructure/Repositories/CatalogoRepository.cs | V1.11
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Planta.Application.Abstractions;
using Planta.Contracts.Catalogos;
using Planta.Contracts.Common;
using Planta.Contracts.Transporte;

namespace Planta.Infrastructure.Repositories;

public sealed class CatalogoRepository : ICatalogoRepository
{
    private readonly string _connString;
    private readonly IMemoryCache _cache;

    public CatalogoRepository(IConfiguration config, IMemoryCache cache)
    {
        _connString = config.GetConnectionString("PlantaDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:PlantaDb");
        _cache = cache;
    }

    // -------------------------
    // Helpers comunes
    // -------------------------
    private static async Task<string> ResolveColumnAsync(SqlConnection cn, string schema, string table, params string[] candidates)
    {
        var inList = string.Join(",", candidates.Select(c => $"N'{c}'"));
        var sql = $@"
SELECT TOP (1) c.name
FROM sys.columns c
JOIN sys.objects o ON o.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE s.name = @schema AND o.name = @table AND c.name IN ({inList})
ORDER BY CASE c.name
  {string.Join("\n", candidates.Select((c, i) => $"WHEN N'{c}' THEN {i}"))}
  ELSE 999
END;";
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@schema", SqlDbType.NVarChar, 128) { Value = schema });
        cmd.Parameters.Add(new SqlParameter("@table", SqlDbType.NVarChar, 128) { Value = table });
        var obj = await cmd.ExecuteScalarAsync();
        if (obj is string name) return name;
        throw new InvalidOperationException($"No pude resolver columnas candidatas en [{schema}].[{table}]: {string.Join(", ", candidates)}");
    }

    // =====================================================================
    // Productos
    // =====================================================================
    public async Task<PagedResult<ProductoDto>> ListarProductosAsync(
        PagedRequest page, string? search, string? orderBy = null, bool desc = false, CancellationToken ct = default)
    {
        var skip = (page.Page <= 1 ? 0 : (page.Page - 1) * page.PageSize);
        var take = page.PageSize <= 0 ? 20 : page.PageSize;

        var orderColumn = (orderBy ?? "nombre").ToLowerInvariant() switch
        {
            "unidad" => "u.Nombre",
            _ => "p.Nombre"
        };
        var orderDir = desc ? "DESC" : "ASC";

        const string WHERE = @"
WHERE 1=1
  AND (@search IS NULL OR p.Nombre LIKE '%' + @search + '%')";

        var sqlCount = $@"
SELECT COUNT(1)
FROM cat.Producto p
{WHERE};";

        var sqlPage = $@"
SELECT 
    p.Id,
    CAST(NULL AS nvarchar(50))    AS Codigo,
    p.Nombre                      AS Nombre,
    u.Nombre                      AS Unidad,
    p.Vendible                    AS vendible_en_cantera
FROM cat.Producto p
LEFT JOIN cat.Unidad u ON u.Id = p.UnidadId
{WHERE}
ORDER BY {orderColumn} {orderDir}
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var key = $"prod:{page.Page}:{take}:{search}:{orderBy}:{desc}";
        if (_cache.TryGetValue(key, out PagedResult<ProductoDto>? cached))
            return cached!;

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        int total;
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            total = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
        }

        var items = new List<ProductoDto>(take);
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take });

            using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            while (await rd.ReadAsync(ct))
            {
                items.Add(new ProductoDto
                {
                    Id = rd.GetInt32(0),
                    Codigo = rd.IsDBNull(1) ? null : rd.GetString(1),
                    Nombre = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Unidad = rd.IsDBNull(3) ? null : rd.GetString(3),
                    VendibleEnCantera = !rd.IsDBNull(4) && rd.GetBoolean(4)
                });
            }
        }

        var result = new PagedResult<ProductoDto>(items, page.Page, take, total);

        _cache.Set(key, result, TimeSpan.FromSeconds(60));
        return result;
    }

    // =====================================================================
    // Unidades
    // =====================================================================
    public async Task<PagedResult<UnidadDto>> ListarUnidadesAsync(
        PagedRequest page, string? search, CancellationToken ct = default)
    {
        var skip = (page.Page <= 1 ? 0 : (page.Page - 1) * page.PageSize);
        var take = page.PageSize <= 0 ? 20 : page.PageSize;

        const string WHERE = @"
WHERE 1=1
  AND (@search IS NULL OR u.Nombre LIKE '%' + @search + '%' OR u.Codigo LIKE '%' + @search + '%')";

        var sqlCount = $@"
SELECT COUNT(1)
FROM cat.Unidad u
{WHERE};";

        var sqlPage = $@"
SELECT u.Id, u.Codigo, u.Nombre
FROM cat.Unidad u
{WHERE}
ORDER BY u.Nombre
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var key = $"unid:{page.Page}:{take}:{search}";
        if (_cache.TryGetValue(key, out PagedResult<UnidadDto>? cached))
            return cached!;

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        int total;
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            total = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
        }

        var items = new List<UnidadDto>(take);
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take });

            using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            while (await rd.ReadAsync(ct))
            {
                items.Add(new UnidadDto
                {
                    Id = rd.GetInt32(0),
                    Codigo = rd.IsDBNull(1) ? null : rd.GetString(1),
                    Nombre = rd.IsDBNull(2) ? null : rd.GetString(2)
                });
            }
        }

        var result = new PagedResult<UnidadDto>(items, page.Page, take, total);

        _cache.Set(key, result, TimeSpan.FromSeconds(60));
        return result;
    }

    // =====================================================================
    // Vehículos
    // =====================================================================
    public async Task<PagedResult<VehiculoDto>> ListarVehiculosAsync(
        PagedRequest page, string? search, int? claseId = null, bool soloActivos = true, CancellationToken ct = default)
    {
        var skip = (page.Page <= 1 ? 0 : (page.Page - 1) * page.PageSize);
        var take = page.PageSize <= 0 ? 20 : page.PageSize;

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        const string sch = "tpt";
        const string tv = "Vehiculo";
        const string tc = "ClaseVehiculo";

        var colIdVeh = await ResolveColumnAsync(cn, sch, tv, "Id", "VehiculoId", "IdVehiculo");
        var colPlaca = await ResolveColumnAsync(cn, sch, tv, "Placa", "Matricula", "PlacaVehiculo");
        var colClaseId = await ResolveColumnAsync(cn, sch, tv, "ClaseVehiculoId", "ClaseId", "IdClaseVehiculo");
        var colActivoV = await ResolveColumnAsync(cn, sch, tv, "Activo", "Habilitado", "Estado");

        var colIdClase = await ResolveColumnAsync(cn, sch, tc, "Id", "ClaseVehiculoId", "IdClase");
        var colNomClase = await ResolveColumnAsync(cn, sch, tc, "Nombre", "Descripcion", "NomClase");

        var where = @"
WHERE 1=1
  AND (@search IS NULL OR v.[{0}] LIKE '%' + @search + '%')
  AND (@claseId IS NULL OR v.[{1}] = @claseId)
  AND (@soloActivos = 0 OR v.[{2}] = 1)";

        var sqlCount = $@"
SELECT COUNT(1)
FROM [{sch}].[{tv}] v
{string.Format(where, colPlaca, colClaseId, colActivoV)};";

        var sqlPage = $@"
SELECT 
  v.[{colIdVeh}]    AS Id,
  v.[{colPlaca}]    AS Placa,
  c.[{colNomClase}] AS Clase,
  v.[{colActivoV}]  AS Activo
FROM [{sch}].[{tv}] v
LEFT JOIN [{sch}].[{tc}] c ON c.[{colIdClase}] = v.[{colClaseId}]
{string.Format(where, colPlaca, colClaseId, colActivoV)}
ORDER BY v.[{colPlaca}]
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var key = $"veh:{page.Page}:{take}:{search}:{claseId}:{soloActivos}";
        if (_cache.TryGetValue(key, out PagedResult<VehiculoDto>? cached))
            return cached!;

        int total;
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 120) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@claseId", SqlDbType.Int) { Value = (object?)claseId ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@soloActivos", SqlDbType.Bit) { Value = soloActivos });
            total = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
        }

        var items = new List<VehiculoDto>(take);
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 120) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@claseId", SqlDbType.Int) { Value = (object?)claseId ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@soloActivos", SqlDbType.Bit) { Value = soloActivos });
            cmd.Parameters.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take });

            using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            while (await rd.ReadAsync(ct))
            {
                items.Add(new VehiculoDto
                {
                    Id = rd.GetInt32(0),
                    Placa = rd.IsDBNull(1) ? null : rd.GetString(1),
                    Clase = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Activo = !rd.IsDBNull(3) && rd.GetBoolean(3)
                });
            }
        }

        var result = new PagedResult<VehiculoDto>(items, page.Page, take, total);
        _cache.Set(key, result, TimeSpan.FromSeconds(60));
        return result;
    }

    // =====================================================================
    // Conductores
    // =====================================================================
    public async Task<PagedResult<ConductorDto>> ListarConductoresAsync(
        PagedRequest page, string? search, bool soloActivos = true, CancellationToken ct = default)
    {
        var skip = (page.Page <= 1 ? 0 : (page.Page - 1) * page.PageSize);
        var take = page.PageSize <= 0 ? 20 : page.PageSize;

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        const string sch = "tpt";
        const string tb = "Conductor";

        var colId = await ResolveColumnAsync(cn, sch, tb, "Id", "ConductorId", "IdConductor");
        var colDoc = await ResolveColumnAsync(cn, sch, tb, "Documento", "Cedula", "Identificacion", "DNI");
        var colNombre = await ResolveColumnAsync(cn, sch, tb, "Nombre", "Nombres", "NombreCompleto");
        var colActivo = await ResolveColumnAsync(cn, sch, tb, "Activo", "Habilitado", "Estado");

        var where = @"
WHERE 1=1
  AND (@search IS NULL OR [{0}] LIKE '%' + @search + '%' OR [{1}] LIKE '%' + @search + '%')
  AND (@soloActivos = 0 OR [{2}] = 1)";

        var sqlCount = $@"
SELECT COUNT(1)
FROM [{sch}].[{tb}]
{string.Format(where, colNombre, colDoc, colActivo)};";

        var sqlPage = $@"
SELECT 
  [{colId}]     AS Id,
  [{colDoc}]    AS Documento,
  [{colNombre}] AS Nombre,
  [{colActivo}] AS Activo
FROM [{sch}].[{tb}]
{string.Format(where, colNombre, colDoc, colActivo)}
ORDER BY [{colNombre}]
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var key = $"cond:{page.Page}:{take}:{search}:{soloActivos}";
        if (_cache.TryGetValue(key, out PagedResult<ConductorDto>? cached))
            return cached!;

        int total;
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 120) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@soloActivos", SqlDbType.Bit) { Value = soloActivos });
            total = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
        }

        var items = new List<ConductorDto>(take);
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 120) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@soloActivos", SqlDbType.Bit) { Value = soloActivos });
            cmd.Parameters.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take });

            using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            while (await rd.ReadAsync(ct))
            {
                items.Add(new ConductorDto
                {
                    Id = rd.GetInt32(0),
                    Documento = rd.IsDBNull(1) ? null : rd.GetString(1),
                    Nombre = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Activo = !rd.IsDBNull(3) && rd.GetBoolean(3)
                });
            }
        }

        var result = new PagedResult<ConductorDto>(items, page.Page, take, total);
        _cache.Set(key, result, TimeSpan.FromSeconds(60));
        return result;
    }

    // =====================================================================
    // Plantas
    // =====================================================================
    public async Task<PagedResult<PlantaDto>> ListarPlantasAsync(
        PagedRequest page, string? search, CancellationToken ct = default)
    {
        var skip = (page.Page <= 1 ? 0 : (page.Page - 1) * page.PageSize);
        var take = page.PageSize <= 0 ? 20 : page.PageSize;

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        const string sch = "cfg";
        const string tb = "Planta";

        var colId = await ResolveColumnAsync(cn, sch, tb, "Id", "PlantaId", "IdPlanta");
        var colNombre = await ResolveColumnAsync(cn, sch, tb, "Nombre", "Descripcion", "NomPlanta");

        var where = @"
WHERE 1=1
  AND (@search IS NULL OR [{0}] LIKE '%' + @search + '%')";

        var sqlCount = $@"
SELECT COUNT(1)
FROM [{sch}].[{tb}]
{string.Format(where, colNombre)};";

        var sqlPage = $@"
SELECT [{colId}] AS Id, [{colNombre}] AS Nombre
FROM [{sch}].[{tb}]
{string.Format(where, colNombre)}
ORDER BY [{colNombre}]
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var key = $"pl:{page.Page}:{take}:{search}";
        if (_cache.TryGetValue(key, out PagedResult<PlantaDto>? cached))
            return cached!;

        int total;
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            total = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
        }

        var items = new List<PlantaDto>(take);
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take });

            using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            while (await rd.ReadAsync(ct))
            {
                items.Add(new PlantaDto
                {
                    Id = rd.GetInt32(0),
                    Nombre = rd.IsDBNull(1) ? null : rd.GetString(1)
                });
            }
        }

        var result = new PagedResult<PlantaDto>(items, page.Page, take, total);
        _cache.Set(key, result, TimeSpan.FromSeconds(60));
        return result;
    }

    // =====================================================================
    // Almacenes
    // =====================================================================
    public async Task<PagedResult<AlmacenDto>> ListarAlmacenesAsync(
        PagedRequest page, string? search, int? plantaId = null, CancellationToken ct = default)
    {
        var skip = (page.Page <= 1 ? 0 : (page.Page - 1) * page.PageSize);
        var take = page.PageSize <= 0 ? 20 : page.PageSize;

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        const string sch = "cfg";
        const string tb = "Almacen";

        var colId = await ResolveColumnAsync(cn, sch, tb, "Id", "AlmacenId", "IdAlmacen");
        var colPlantaId = await ResolveColumnAsync(cn, sch, tb, "PlantaId", "IdPlanta", "Planta");
        var colNombre = await ResolveColumnAsync(cn, sch, tb, "Nombre", "Descripcion", "NomAlmacen");

        var where = @"
WHERE 1=1
  AND (@search IS NULL OR [{0}] LIKE '%' + @search + '%')
  AND (@plantaId IS NULL OR [{1}] = @plantaId)";

        var sqlCount = $@"
SELECT COUNT(1)
FROM [{sch}].[{tb}]
{string.Format(where, colNombre, colPlantaId)};";

        var sqlPage = $@"
SELECT [{colId}] AS Id, [{colPlantaId}] AS PlantaId, [{colNombre}] AS Nombre
FROM [{sch}].[{tb}]
{string.Format(where, colNombre, colPlantaId)}
ORDER BY [{colNombre}]
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var key = $"alm:{page.Page}:{take}:{search}:{plantaId}";
        if (_cache.TryGetValue(key, out PagedResult<AlmacenDto>? cached))
            return cached!;

        int total;
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@plantaId", SqlDbType.Int) { Value = (object?)plantaId ?? DBNull.Value });
            total = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
        }

        var items = new List<AlmacenDto>(take);
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@plantaId", SqlDbType.Int) { Value = (object?)plantaId ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take });

            using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            while (await rd.ReadAsync(ct))
            {
                items.Add(new AlmacenDto
                {
                    Id = rd.GetInt32(0),
                    PlantaId = rd.GetInt32(1),
                    Nombre = rd.IsDBNull(2) ? null : rd.GetString(2)
                });
            }
        }

        var result = new PagedResult<AlmacenDto>(items, page.Page, take, total);
        _cache.Set(key, result, TimeSpan.FromSeconds(60));
        return result;
    }

    // =====================================================================
    // Clientes (crm)
    // =====================================================================
    public async Task<PagedResult<ClienteDto>> ListarClientesAsync(
        PagedRequest page, string? search, CancellationToken ct = default)
    {
        var skip = (page.Page <= 1 ? 0 : (page.Page - 1) * page.PageSize);
        var take = page.PageSize <= 0 ? 20 : page.PageSize;

        const string WHERE = @"
WHERE 1=1
  AND (@search IS NULL OR @search = '' OR c.Nombre LIKE '%' + @search + '%' OR c.NIT LIKE '%' + @search + '%')";

        var sqlCount = $@"
SELECT COUNT(1)
FROM crm.Cliente c
{WHERE};";

        var sqlPage = $@"
SELECT c.Id, c.Nombre, c.NIT, c.Activo, c.ListaPrecios
FROM crm.Cliente c
{WHERE}
ORDER BY c.Nombre
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var key = $"cli:{page.Page}:{take}:{search}";
        if (_cache.TryGetValue(key, out PagedResult<ClienteDto>? cached))
            return cached!;

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        int total;
        using (var cmd = new SqlCommand(sqlCount, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            total = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
        }

        var items = new List<ClienteDto>(take);
        using (var cmd = new SqlCommand(sqlPage, cn))
        {
            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take });

            using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            while (await rd.ReadAsync(ct))
            {
                items.Add(new ClienteDto
                {
                    Id = rd.GetInt32(0),
                    Nombre = rd.GetString(1),
                    NIT = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Activo = !rd.IsDBNull(3) && rd.GetBoolean(3),
                    ListaPrecios = rd.IsDBNull(4) ? null : rd.GetString(4)
                });
            }
        }

        var result = new PagedResult<ClienteDto>(items, page.Page, take, total);
        _cache.Set(key, result, TimeSpan.FromSeconds(30));
        return result;
    }

    public async Task<ClienteDto?> ObtenerClientePorIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = @"SELECT Id, Nombre, NIT, Activo, ListaPrecios FROM crm.Cliente WHERE Id = @id;";
        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });

        using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;

        return new ClienteDto
        {
            Id = rd.GetInt32(0),
            Nombre = rd.GetString(1),
            NIT = rd.IsDBNull(2) ? null : rd.GetString(2),
            Activo = !rd.IsDBNull(3) && rd.GetBoolean(3),
            ListaPrecios = rd.IsDBNull(4) ? null : rd.GetString(4)
        };
    }

    // =====================================================================
    // Autocompletar vehículos por placa (con último conductor)
    // =====================================================================
    public async Task<IReadOnlyList<VehiculoAutocompleteDto>> AutocompletarVehiculosAsync(
        string? q, int take = 10, CancellationToken ct = default)
    {
        var key = $"veh-ac:{q}:{take}";
        if (_cache.TryGetValue(key, out IReadOnlyList<VehiculoAutocompleteDto>? cached))
            return cached!;

        var items = new List<VehiculoAutocompleteDto>(take <= 0 ? 10 : take);

        using var cn = new SqlConnection(_connString);
        await cn.OpenAsync(ct);

        const string sql = @"
;WITH v AS
(
    SELECT TOP (@take)
        v.Id     AS VehiculoId,
        v.Placa  AS Placa,
        CASE WHEN @q IS NULL OR @q = '' THEN 0
             WHEN v.Placa LIKE @q + '%' THEN 0 ELSE 1 END AS RankPrefix
    FROM tpt.Vehiculo v
    WHERE (@q IS NULL OR @q = '' OR v.Placa LIKE '%' + @q + '%')
    ORDER BY RankPrefix, v.Placa
)
SELECT 
    v.VehiculoId,
    v.Placa,
    c.Id       AS ConductorId,
    c.Nombre   AS ConductorNombre
FROM v
OUTER APPLY (
    SELECT TOP (1) h.ConductorId
    FROM tpt.VehiculoConductorHist h
    WHERE h.VehiculoId = v.VehiculoId
    ORDER BY 
        CASE WHEN h.Hasta IS NULL THEN 1 ELSE 0 END DESC,  -- vigentes primero
        h.Hasta DESC,
        h.Desde DESC
) lastH
LEFT JOIN tpt.Conductor c ON c.Id = lastH.ConductorId;";

        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@q", SqlDbType.NVarChar, 200) { Value = (object?)q ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take <= 0 ? 10 : take });

        using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            items.Add(new VehiculoAutocompleteDto
            {
                VehiculoId = rd.GetInt32(0),
                Placa = rd.IsDBNull(1) ? null : rd.GetString(1),
                ConductorId = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2),
                ConductorNombre = rd.IsDBNull(3) ? null : rd.GetString(3)
            });
        }

        _cache.Set(key, items, TimeSpan.FromSeconds(30));
        return items;
    }
}
