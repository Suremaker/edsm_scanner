using System;
using System.Linq;

namespace EdsmScanner.Plotting
{
    internal class SystemDistance : IComparable<SystemDistance>
    {
        public JourneySystem First { get; }
        public JourneySystem Second { get; }
        public double Distance { get; }

        public SystemDistance(JourneySystem first, JourneySystem second)
        {
            First = first;
            Second = second;
            Distance = first.Coords.Distance(second.Coords);
        }

        public int CompareTo(SystemDistance? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Distance.CompareTo(other.Distance);
        }

        public bool CanBeUsed(bool canCloseTheLoop)
        {
            if (First.Connections.Count == 2 || Second.Connections.Count == 2)
                return false;

            if (First.Connections.Count == 0 || Second.Connections.Count == 0)
                return true;

            var last = Second.Traverse().LastOrDefault();
            if (last == First) return canCloseTheLoop;
            return true;
        }
    }
}