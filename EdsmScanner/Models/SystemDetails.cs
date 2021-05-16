using System;
using System.Linq;

namespace EdsmScanner.Models
{
    internal class SystemDetails
    {
        private SystemBody[]? _filteredBodies;
        public long? Id64 { get; set; }
        public int? BodyCount { get; set; }
        public int StarCount => Bodies.Count(b => b.Type.Equals("star", StringComparison.OrdinalIgnoreCase));
        public SystemBody[] Bodies { get; set; } = Array.Empty<SystemBody>();
        public string Url { get; set; } = string.Empty;

        public bool IsNotFullyDiscovered => BodyCount == null || BodyCount > Bodies.Length;
        public SystemRef? Ref { get; set; }
        public override string ToString() => $"{Ref}";

        public decimal PlottedDistance { get; set; }

        public SystemBody[] FilteredBodies => _filteredBodies ?? Bodies;

        public void ApplyBodyFilter(Func<IQueryable<SystemBody>, IQueryable<SystemBody>> filter)
        {
            _filteredBodies = filter(Bodies.AsQueryable()).ToArray();
        }
    }
}