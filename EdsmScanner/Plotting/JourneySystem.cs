using System.Collections.Generic;
using System.Linq;
using EdsmScanner.Models;

namespace EdsmScanner.Plotting
{
    class JourneySystem
    {
        public JourneySystem(SystemDetails details)
        {
            Details = details;
        }

        public SystemDetails Details { get; }
        public CoordF Coords => Details.Ref.Coords;
        public IList<JourneySystem> Connections { get; } = new List<JourneySystem>();

        public IEnumerable<JourneySystem> Traverse()
        {
            JourneySystem? last = null;
            var current = this;
            
            while (true)
            {
                var next = current.Connections.FirstOrDefault(x => x != last);

                if (next == null) yield break;

                yield return next;
                
                if (next == this)
                    yield break;

                last = current;
                current = next;
            }
        }

        public override string ToString()
        {
            return $"{Details.Ref.Name} [{string.Join(", ", Connections.Select(c => c.Details.Ref.Name))}]";
        }
    }
}