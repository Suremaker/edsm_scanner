using System;
using System.Linq;

namespace EdsmScanner.Models
{
    internal class SystemDetails
    {
        private SystemBody[]? _filteredBodies;
        [Queryable]
        public long? Id64 { get; set; }
        [Queryable]
        public int DiscoveredStars => Bodies?.Count(b => b.Type.Equals("star", StringComparison.OrdinalIgnoreCase)) ?? 0;
        [Queryable]
        public int DiscoveredBodies => Bodies?.Length ?? 0;
        [Queryable]
        public bool IsFullyDiscovered => BodyCount.GetValueOrDefault() > 0 && BodyCount <= DiscoveredBodies;

        /// <summary>
        /// Expected count
        /// </summary>
        public int? BodyCount { get; set; }
        public SystemBody[]? Bodies { get; set; } = Array.Empty<SystemBody>();
        public string Url { get; set; } = string.Empty;
        public SystemRef? Ref { get; set; }

        public override string ToString() => $"{Ref}";
        public decimal PlottedDistance { get; set; }
        public SystemBody[] FilteredBodies => _filteredBodies ?? Bodies ?? Array.Empty<SystemBody>();

        public void ApplyBodyFilter(Func<IQueryable<SystemBody>, IQueryable<SystemBody>> filter)
        {
            _filteredBodies = filter((Bodies ?? Array.Empty<SystemBody>()).AsQueryable()).ToArray();
        }
    }
}