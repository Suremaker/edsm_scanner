using System;
using System.Linq;
using System.Threading.Tasks;
using EdsmScanner.Clients;
using EdsmScanner.Plotting;
using EdsmScanner.Search;
using EdsmScanner.Writers;

namespace EdsmScanner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("EdsmScanner.exe [origin-system-name] [scan-radius] [plot-journey]");
                Console.WriteLine("scan-radius is optional and set to 50");
                Console.WriteLine("plot-journey is optional and set to N");
                return;
            }

            var originSystem = args[0];
            var radius = ParseRadius(args);
            var plotJourney = ShallPlotJourney(args);

            using var client = new EdsmClient(new SystemCache());
            var foundSystems = await new SystemResolver(client).ResolveSystemsAround(originSystem, radius);

            var filteredSystems = new SystemFilter().FindPartiallyDiscovered(foundSystems);

            var remainingSystems = foundSystems.Except(filteredSystems).ToArray();
            await new VisitedSystemIdsWriter().WriteVisitedSystems(originSystem, remainingSystems);

            var orderedPartialSystems = new SystemOrderer(filteredSystems, plotJourney).Order();
            await new SystemListWriter().WriteSystemList(originSystem, orderedPartialSystems, plotJourney);
        }

        private static int ParseRadius(string[] args)
        {
            return args.Length > 1 && int.TryParse(args[1], out var r) ? r : 50;
        }

        private static bool ShallPlotJourney(string[] args)
        {
            var enabled = new[] { "y", "yes", "1", "t", "true" };
            return args.Length > 2 && enabled.Contains(args[2], StringComparer.OrdinalIgnoreCase);
        }
    }
}
