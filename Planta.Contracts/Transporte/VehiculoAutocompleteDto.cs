namespace Planta.Contracts.Transporte
{
    public sealed class VehiculoAutocompleteDto
    {
        public int VehiculoId { get; set; }
        public string? Placa { get; set; }
        public int? ConductorId { get; set; }
        public string? ConductorNombre { get; set; }
    }
}
