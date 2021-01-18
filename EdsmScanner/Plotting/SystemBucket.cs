using System.Collections.Generic;
using System.Linq;

namespace EdsmScanner.Plotting
{
    internal class SystemBucket
    {
        private readonly HashSet<JourneySystem> _systems = new();
        private IEnumerator<SystemDistance>? _distanceEnumerator;

        public SystemBucket()
        {
        }

        public SystemBucket(IEnumerable<JourneySystem> systems)
        {
            foreach (var sys in systems)
                Add(sys);
        }

        public void Add(JourneySystem system)
        {
            _systems.Add(system);
        }

        public override string ToString() => $"{_systems.Count}";

        public void InitializeDistances()
        {
            _distanceEnumerator = GetDistances().OrderBy(d => d.Distance).GetEnumerator();
            MoveToNextDistance();
        }

        private IEnumerable<SystemDistance> GetDistances()
        {
            var sys = _systems.ToArray();
            for (int i = 0; i < sys.Length; ++i)
                for (int j = i + 1; j < sys.Length; ++j)
                    yield return new SystemDistance(sys[i], sys[j]);
        }

        public SystemDistance? GetCurrentDistance(bool canCloseTheLoop)
        {
            while (_distanceEnumerator != null)
            {
                var current = _distanceEnumerator.Current;

                if (current.CanBeUsed(canCloseTheLoop))
                    return current;
                MoveToNextDistance();
            }

            return null;
        }

        public void MoveToNextDistance()
        {
            if (_distanceEnumerator != null && _distanceEnumerator.MoveNext()) return;
            _distanceEnumerator = null;
        }
    }
}