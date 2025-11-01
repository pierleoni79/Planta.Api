#nullable enable
using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Requests;
using Planta.Mobile.Models.Recibos;

namespace Planta.Mobile.Services.Api;

public static class RecibosApiExtensions
{
    public static async Task<Guid> CreateFullAsync(
        this IRecibosApi api,
        NuevoReciboForm form,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        if (api is null) throw new ArgumentNullException(nameof(api));
        if (form is null) throw new ArgumentNullException(nameof(form));

        // 0) Id para idempotencia
        var id = form.Id == Guid.Empty ? Guid.NewGuid() : form.Id;

        // 1) Crear (👈 aquí el cambio: POCO → object initializer)
        var crearReq = new ReciboCreateRequest
        {
            Id = id,
            EmpresaId = form.EmpresaId,
            DestinoTipo = form.Destino, // Planta.Contracts.Enums.DestinoTipo
            Cantidad = form.Cantidad,
            IdempotencyKey = string.IsNullOrWhiteSpace(form.IdempotencyKey)
                ? idempotencyKey
                : form.IdempotencyKey!,
            PlacaSnapshot = form.Placa?.Trim().ToUpperInvariant(),
            ConductorNombreSnapshot = form.ConductorNombreSnapshot,
            ReciboFisicoNumero = form.ReciboFisicoNumero
        };

        var createdId = await api.CrearAsync(crearReq, ct).ConfigureAwait(false) ?? id;

        // 2) Vincular transporte (solo si hay algo que vincular)
        if (form.VehiculoId.HasValue || form.ConductorId.HasValue
            || !string.IsNullOrWhiteSpace(form.Placa)
            || !string.IsNullOrWhiteSpace(form.ConductorNombreSnapshot))
        {
            var vTrans = new VincularTransporteRequest(
                Id: createdId,
                VehiculoId: form.VehiculoId,
                ConductorId: form.ConductorId,
                PlacaSnapshot: form.Placa?.Trim().ToUpperInvariant(),
                ConductorNombreSnapshot: form.ConductorNombreSnapshot
            );
            _ = await api.VincularTransporteAsync(vTrans, ct).ConfigureAwait(false);
        }

        // 3) Vincular origen (si aplica)
        if (form.PlantaId.HasValue || form.AlmacenOrigenId.HasValue || form.ClienteId.HasValue)
        {
            var vOrg = new VincularOrigenRequest(
                Id: createdId,
                PlantaId: form.PlantaId,
                AlmacenOrigenId: form.AlmacenOrigenId,
                ClienteId: form.ClienteId
            );
            _ = await api.VincularOrigenAsync(vOrg, ct).ConfigureAwait(false);
        }

        // 4) Registrar material (si aplica)
        if (form.MaterialId is int mid && mid > 0)
        {
            var reg = new RegistrarMaterialRequest(
                Id: createdId,
                MaterialId: mid,
                Cantidad: form.Cantidad
            );
            _ = await api.RegistrarMaterialAsync(reg, ct).ConfigureAwait(false);
        }

        return createdId;
    }
}
