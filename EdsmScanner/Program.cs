using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EdsmScanner.Clients;
using EdsmScanner.Models;
using EdsmScanner.Plotting;
using EdsmScanner.Search;

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

            using var client = new EdsmClient();
            var systemDetails = await new SystemResolver(client).ResolveSystemsAround(originSystem, radius);

            var discovered = systemDetails.Where(x => !x.IsNotFullyDiscovered).ToArray();
            await WriteDiscoveredOutput(originSystem, discovered);

            var partiallyDiscovered = GetPartiallyDiscoveredSystems(systemDetails, plotJourney);
            await WritePartialOutput(originSystem, partiallyDiscovered, plotJourney);
        }

        private static SystemDetails[] GetPartiallyDiscoveredSystems(SystemDetails[] systemDetails, bool plotJourney)
        {
            var filteredSystems = systemDetails.Where(x => x.IsNotFullyDiscovered);

            return plotJourney
                ? new JourneyPlotter(filteredSystems).Plot()
                : filteredSystems.OrderBy(d => d.Ref?.Distance).ToArray();
        }

        private static async Task WriteDiscoveredOutput(string originSystem, SystemDetails[] discovered)
        {
            var path = SanitizePath($"discovered_{originSystem}.txt");
            await using var discoveredWriter = new StreamWriter(path);
            int lines = 0;
            foreach (var sys in discovered)
            {
                if (sys.Id64.HasValue)
                {
                    await discoveredWriter.WriteLineAsync(sys.Id64.ToString());
                    ++lines;
                }
            }
            Console.WriteLine($"Generated {path} with {lines} systems.");
        }


        private static async Task WritePartialOutput(string originSystem, SystemDetails[] partiallyDiscoveredSystems, bool journeyPlotted)
        {
            var path = SanitizePath($"partial_{originSystem}.txt");
            await using var partialWriter = new StreamWriter(path);

            await partialWriter.WriteLineAsync(journeyPlotted ? $"# distances calculated to previous system, starting from: {originSystem}" : $"# distances calculated to origin system: {originSystem}");

            foreach (var sys in partiallyDiscoveredSystems)
            {
                var distance = journeyPlotted ? sys.PlottedDistance : sys.Ref.Distance;
                await partialWriter.WriteLineAsync($"{sys.Ref.Name} [{distance:F2}ly] ({sys.Ref.BodyCount?.ToString() ?? "?"} bodies / {sys.Bodies?.Length.ToString() ?? "?"} discovered) => {sys.Url}");
            }

            Console.WriteLine($"Generated {path} with {partiallyDiscoveredSystems.Length} systems.");
        }

        private static string SanitizePath(string fileName)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        private static async Task<SystemRef[]> SearchSystems(EdsmClient client, string originSystem, int radius)
        {
            Console.WriteLine($"Searching for systems in {radius}ly distance from: {originSystem}");
            var systems = await client.SearchSystems(originSystem, radius);

            Console.WriteLine($"Found systems: {systems.Length}");
            return systems;
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
