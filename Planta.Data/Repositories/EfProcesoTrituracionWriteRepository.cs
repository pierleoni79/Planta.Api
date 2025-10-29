// Ruta: /Planta.Data/Repositories/EfProcesoTrituracionWriteRepository.cs | V1.3
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Planta.Data.Context;
using Planta.Domain.Produccion;

namespace Planta.Data.Repositories;

public sealed class EfProcesoTrituracionWriteRepository : IProcesoTrituracionWriteRepository
{
    private readonly PlantaDbContext _db;
    public EfProcesoTrituracionWriteRepository(PlantaDbContext db) => _db = db;

    public async Task<ProcesoTrituracion> AddProcesoAsync(ProcesoTrituracion proceso, CancellationToken ct = default)
    {
        if (proceso is null) throw new ArgumentNullException(nameof(proceso));
        if (proceso.PesoEntrada <= 0) throw new ArgumentOutOfRangeException(nameof(proceso.PesoEntrada), "Debe ser > 0");
        if (proceso.ReciboId == Guid.Empty) throw new ArgumentException("ReciboId inválido.", nameof(proceso));

        await _db.AddAsync(proceso, ct).ConfigureAwait(false);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return proceso;
    }

    public async Task AddSalidaAsync(ProcesoTrituracionSalida salida, decimal cantidadM3, CancellationToken ct = default)
    {
        if (salida is null) throw new ArgumentNullException(nameof(salida));

        // ✅ ProcesoId es GUID en tu modelo actual
        if (salida.ProcesoId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(salida.ProcesoId), "ProcesoId inválido");

        if (salida.ProductoId <= 0)
            throw new ArgumentOutOfRangeException(nameof(salida.ProductoId), "ProductoId inválido");

        if (cantidadM3 <= 0)
            throw new ArgumentOutOfRangeException(nameof(cantidadM3), "Debe ser > 0");

        var cant = decimal.Round(cantidadM3, 3, MidpointRounding.AwayFromZero);

        await _db.AddAsync(salida, ct).ConfigureAwait(false);                 // trackear
        _db.Entry(salida).Property<decimal>("CantidadM3").CurrentValue = cant; // propiedad sombra
        // No guardamos aquí: usar SaveAsync() para batch
    }

    public async Task AddSalidasAsync(IEnumerable<(ProcesoTrituracionSalida salida, decimal cantidadM3)> salidas, CancellationToken ct = default)
    {
        foreach (var (s, c) in salidas)
            await AddSalidaAsync(s, c, ct).ConfigureAwait(false);
    }

    public async Task<ProcesoTrituracion> AddProcesoConSalidasAsync(
        ProcesoTrituracion proceso,
        IEnumerable<(int productoId, decimal cantidadM3)> salidas,
        CancellationToken ct = default)
    {
        if (proceso is null) throw new ArgumentNullException(nameof(proceso));
        if (proceso.PesoEntrada <= 0) throw new ArgumentOutOfRangeException(nameof(proceso.PesoEntrada));
        if (proceso.ReciboId == Guid.Empty) throw new ArgumentException("ReciboId inválido.", nameof(proceso));
        if (salidas is null) throw new ArgumentNullException(nameof(salidas));

        await _db.AddAsync(proceso, ct).ConfigureAwait(false);

        foreach (var (productoId, cantidadM3) in salidas)
        {
            if (productoId <= 0) throw new ArgumentOutOfRangeException(nameof(productoId));
            if (cantidadM3 <= 0) throw new ArgumentOutOfRangeException(nameof(cantidadM3));

            var det = new ProcesoTrituracionSalida
            {
                ProcesoId = proceso.Id,   // si usas navegación: Proceso = proceso
                ProductoId = productoId
            };
            await _db.AddAsync(det, ct).ConfigureAwait(false);
            _db.Entry(det).Property<decimal>("CantidadM3").CurrentValue =
                decimal.Round(cantidadM3, 3, MidpointRounding.AwayFromZero);
        }

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return proceso;
    }

    public Task<int> SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
