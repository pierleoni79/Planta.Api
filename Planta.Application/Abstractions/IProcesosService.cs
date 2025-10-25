// Ruta: /Planta.Application/Abstractions/IProcesosService.cs | V1.0
using System;
using System.Threading;
using System.Threading.Tasks;
using Planta.Contracts.Procesos;

namespace Planta.Application.Abstractions
{
    public interface IProcesosService
    {
        Task<ProcesoResultDto> ProcesarTrituracionAsync(Guid reciboId, ProcesarTrituracionRequest body, CancellationToken ct);
    }
}
