// Ruta: /Planta.Contracts/Recibos/CheckinRequest.cs | V1.1
using System;

namespace Planta.Contracts.Recibos
{
    public sealed class CheckinRequest
    {
        public double? Latitude { get; set; }            // opcional (-90..90)
        public double? Longitude { get; set; }           // opcional (-180..180)
        public double? AccuracyMeters { get; set; }      // opcional (>=0)
        public string? Source { get; set; }              // "manual" | "geofence" | "device" | null
        public DateTimeOffset? DeviceTimeUtc { get; set; } // opcional
        public string? Notes { get; set; }               // nvarchar(512)
    }
}
