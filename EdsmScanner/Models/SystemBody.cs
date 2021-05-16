using System;

namespace EdsmScanner.Models
{
    internal class SystemBody
    {
        public string Type { get; set; } = string.Empty;
        public string AtmosphereType { get; set; } = string.Empty;
        public decimal? SurfacePressure { get; set; }
        public RingInfo[] Rings { get; set; } = Array.Empty<RingInfo>();
        public string ReserveLevel { get; set; }
        public string Name { get; set; }
        public string SubType { get; set; }
    }
}