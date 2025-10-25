// Ruta: /Planta.Application/Abstractions/ICatalogoRepository.cs | V1.6
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Planta.Contracts.Common;
using Planta.Contracts.Catalogos;
using Planta.Contracts.Transporte;

namespace Planta.Application.Abstractions;

/// <summary>
/// Consultas de catálogos con paginación y filtros.
/// </summary>
public interface ICatalogoRepository
{
    // Productos
    Task<PagedResult<ProductoDto>> ListarProductosAsync(
        PagedRequest page, string? search, string? orderBy = null, bool desc = false, CancellationToken ct = default);

    // Unidades
    Task<PagedResult<UnidadDto>> ListarUnidadesAsync(
        PagedRequest page, string? search, CancellationToken ct = default);

    // Vehículos
    Task<PagedResult<VehiculoDto>> ListarVehiculosAsync(
        PagedRequest page, string? search, int? claseId = null, bool soloActivos = true, CancellationToken ct = default);

    // Conductores
    Task<PagedResult<ConductorDto>> ListarConductoresAsync(
        PagedRequest page, string? search, bool soloActivos = true, CancellationToken ct = default);

    // Plantas
    Task<PagedResult<PlantaDto>> ListarPlantasAsync(
        PagedRequest page, string? search, CancellationToken ct = default);

    // Almacenes
    Task<PagedResult<AlmacenDto>> ListarAlmacenesAsync(
        PagedRequest page, string? search, int? plantaId = null, CancellationToken ct = default);

    // Clientes
    Task<PagedResult<ClienteDto>> ListarClientesAsync(
        PagedRequest page, string? search, CancellationToken ct = default);

    /// <summary>Obtiene un cliente por Id (BD: crm.Cliente.Id = int).</summary>
    Task<ClienteDto?> ObtenerClientePorIdAsync(
        int id, CancellationToken ct = default);

    /// <summary>
    /// Autocompleta vehículos por placa y devuelve el último conductor asociado (vigente o más reciente).
    /// </summary>
    Task<IReadOnlyList<VehiculoAutocompleteDto>> AutocompletarVehiculosAsync(
        string? q, int take = 10, CancellationToken ct = default);
}
