using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using EdsmScanner.Models;

namespace EdsmScanner.Search
{
    internal class SystemFilter
    {
        private readonly string[] _filterSystem;
        private readonly string[] _filterBody;

        public SystemFilter(string[] filterSystem, string[] filterBody)
        {
            _filterSystem = filterSystem;
            _filterBody = filterBody;
        }

        public SystemDetails[] Filter(SystemDetails[] systems)
        {
            var result = FilterSystems(systems);
            result = FilterBodies(result);
            return result;
        }

        private SystemDetails[] FilterBodies(SystemDetails[] systems)
        {
            if (!_filterBody.Any())
                return systems;

            Console.WriteLine($"Searching for systems with bodies matching: {string.Join(" and ", _filterBody)}");

            foreach (var system in systems)
                system.ApplyBodyFilter(bodies => _filterBody.Aggregate(bodies.AsQueryable(), (current, filter) => current.Where(filter)));

            var result = systems.Where(s => s.FilteredBodies.Any()).ToArray();

            Console.WriteLine($"  Found {result.Length} systems");
            return result;
        }

        private SystemDetails[] FilterSystems(SystemDetails[] systems)
        {
            Console.WriteLine($"Searching for systems matching: {string.Join(" and ", _filterSystem)}");

            var result = _filterSystem
                .Aggregate(systems.AsQueryable(), (current, filter) => current.Where(filter))
                .ToArray();

            Console.WriteLine($"  Found {result.Length} systems");
            return result;
        }
    }
}