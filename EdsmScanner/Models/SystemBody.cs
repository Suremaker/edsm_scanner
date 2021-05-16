using System;
using System.Collections.Generic;
using System.Linq;

namespace EdsmScanner.Models
{
    internal class SystemBody
    {
        [Queryable]
        public string Type { get; set; } = string.Empty;
        [Queryable]
        public string AtmosphereType { get; set; } = string.Empty;
        [Queryable]
        public decimal? SurfacePressure { get; set; }
        [Queryable]
        public string ReserveLevel { get; set; }
        [Queryable]
        public string Name { get; set; }
        [Queryable]
        public string SubType { get; set; }
        [Queryable]
        public int DistanceToArrival { get; set; }
        [Queryable]
        public bool? IsScoopable { get; set; }
        [Queryable]
        public int? Age { get; set; }
        [Queryable]
        public string Luminosity { get; set; }
        [Queryable]
        public decimal? SolarMasses { get; set; }
        [Queryable]
        public decimal? SolarRadius { get; set; }
        [Queryable]
        public int? SurfaceTemperature { get; set; }
        [Queryable]
        public string SpectralClass { get; set; }
        [Queryable]
        public bool? IsLandable { get; set; }
        [Queryable]
        public decimal? Gravity { get; set; }
        [Queryable]
        public decimal? EarthMasses { get; set; }
        [Queryable]
        public string VolcanismType { get; set; }
        [Queryable]
        public string TerraformingState { get; set; }
        [Queryable]
        public string[] RingTypes => Rings.Select(r => r.Type).Distinct().ToArray();
        [Queryable]
        public string[] BeltTypes => Belts.Select(r => r.Type).Distinct().ToArray();

        public RingInfo[] Rings { get; set; } = Array.Empty<RingInfo>();
        public RingInfo[] Belts { get; set; } = Array.Empty<RingInfo>();
    }
}