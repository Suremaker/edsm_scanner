using System;

namespace EdsmScanner.Models
{
    internal class SystemBody
    {
        public int? BodyId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string AtmosphereType { get; set; } = string.Empty;
        public decimal? SurfacePressure { get; set; }
        public RingInfo[] Rings { get; set; } = Array.Empty<RingInfo>();
        public RingInfo[] Belts { get; set; } = Array.Empty<RingInfo>();
        public string ReserveLevel { get; set; }
        public string Name { get; set; }
        public string SubType { get; set; }
        public int DistanceToArrival { get; set; }
        public bool? IsScoopable { get; set; }
        public int? Age { get; set; }
        public string Luminosity { get; set; }
        public decimal? SolarMasses { get; set; }
        public decimal? SolarRadius { get; set; }
        public int? SurfaceTemperature { get; set; }
        public string SpectralClass { get; set; }
        public bool? IsLandable { get; set; }
        public decimal? Gravity { get; set; }
        public decimal? EarthMasses { get; set; }
        public string VolcanismType { get; set; }
        public string TerraformingState { get; set; }
    }
}