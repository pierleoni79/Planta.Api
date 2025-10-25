// Ruta: /Planta.Application/Features/Procesos/Trituracion/Procesar/Handler.cs | V1.1
using MediatR;
using Planta.Application.Abstractions;
using Planta.Contracts.Procesos;
using Planta.Domain.Produccion;

namespace Planta.Application.Features.Procesos.Trituracion.Procesar;

public sealed class Handler : IRequestHandler<Command, ProcesoResultDto>
{
    private const double EPSILON = 0.01; // 1%
    private const double DELTA_MIN = 0.1; // ignora ruido

    private readonly IPlantaDbContext _db;
    public Handler(IPlantaDbContext db) => _db = db;

    public async Task<ProcesoResultDto> Handle(Command request, CancellationToken ct)
    {
        double entrada = request.Body.PesoEntrada;
        double salida = request.Body.PesoSalida;
        double residuos = request.Body.Residuos;

        double diffAbs = Math.Abs(entrada - salida - residuos);
        if (diffAbs < DELTA_MIN) diffAbs = 0;

        double balance = entrada == 0 ? 0 : (diffAbs / entrada);
        bool cumple = balance <= EPSILON;

        var entity = new ProcesoTrituracion
        {
            ReciboId = request.ReciboId,
            PesoEntrada = entrada,
            PesoSalida = salida,
            Residuos = residuos,
            BalancePorc = Math.Round(balance * 100, 3),
            Cumple = cumple,
            Epsilon = EPSILON,
            DeltaMin = DELTA_MIN,
            Observaciones = request.Body.Observaciones
        };

        _db.ProcesosTrituracion.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ProcesoResultDto
        {
            BalancePorc = entity.BalancePorc,
            Cumple = entity.Cumple,
            Epsilon = entity.Epsilon,
            DeltaMin = entity.DeltaMin,
            Observaciones = entity.Observaciones
        };
    }
}
