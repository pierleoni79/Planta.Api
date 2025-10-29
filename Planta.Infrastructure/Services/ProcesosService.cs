// Ruta: /Planta.Infrastructure/Services/ProcesosService.cs | V1.3
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Planta.Application.Abstractions;
using Planta.Contracts.Procesos;
using Planta.Data.Context; // si luego persistes con EF

namespace Planta.Infrastructure.Services
{
    /// <summary>
    /// Infrastructure NO debe usar MediatR ni llamar a Features.
    /// Aquí va la lógica/persistencia (EF Core) que invocará el Handler de Application.
    /// </summary>
    public sealed class ProcesosService : IProcesosService
    {
        private readonly PlantaDbContext _db;

        public ProcesosService(PlantaDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// TODO: Implementar persistencia real de la “trituración”:
        ///  - Validar Recibo (estado permitido).
        ///  - Crear/actualizar entidades prd.Proceso / prd.ProcesoDet si aplica.
        ///  - Dejar trazas/observaciones y actualizar estados.
        ///  - Devolver ProcesoResultDto.
        /// Por ahora se deja “no implementado” para mantener capas limpias.
        /// </summary>
        public Task<ProcesoResultDto> ProcesarTrituracionAsync(
            Guid reciboId,
            ProcesarTrituracionRequest body,
            CancellationToken ct)
        {
            return Task.FromException<ProcesoResultDto>(
                new NotImplementedException(
                    "ProcesosService aún no implementado. Flujo correcto: API → IMediator → Handler (Application) → ProcesosService (Infrastructure)."));
        }
    }
}
