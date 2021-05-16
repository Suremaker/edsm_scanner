using System.Linq;
using EdsmScanner.Models;

namespace EdsmScanner.Plotting
{
    class SystemOrderer
    {
        private readonly SystemDetails[] _systems;
        private readonly bool _plotJourney;

        public SystemOrderer(SystemDetails[] systems, bool plotJourney)
        {
            _systems = systems;
            _plotJourney = plotJourney;
        }

        public SystemDetails[] Order()
        {
            return _plotJourney
                ? new JourneyPlotter(_systems).Plot()
                : _systems.OrderBy(d => d.Ref?.Distance).ToArray();
        }
    }
}
