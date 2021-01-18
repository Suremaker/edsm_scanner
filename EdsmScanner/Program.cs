using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EdsmScanner.Plotting;

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

            using var client = new EdsmClient();
            var systems = await SearchSystems(client, originSystem, radius);
            var systemDetails = await new SystemDetailsResolver(client, systems).GetSystemDetails();
            await WriteOutput(originSystem, systemDetails);
            if (ShallPlotJourney(args))
            {
                var journey = new JourneyPlotter(systemDetails.Where(d => d.IsNotFullyDiscovered)).Plot();
                await WriteJourneyOutput(originSystem, journey);
            }
        }

        private static async Task WriteJourneyOutput(string originSystem, IEnumerable<SystemDetails> journey)
        {
            await using var partialWriter = new StreamWriter($"journey_{originSystem}.txt");
            SystemDetails? last = null;
            foreach (var sys in journey)
            {
                var distance = last != null ? (decimal)sys.Ref.Coords.Distance(last.Ref.Coords) : sys.Ref.Distance;
                await partialWriter.WriteLineAsync($"{sys.Ref.Name} [{distance:F2}ly] ({sys.Ref.BodyCount?.ToString() ?? "?"} bodies / {sys.Bodies?.Length.ToString() ?? "?"} discovered) => {sys.Url}");
                last = sys;
            }
            Console.WriteLine("Output journey file generated.");
        }

        private static async Task<SystemRef[]> SearchSystems(EdsmClient client, string originSystem, int radius)
        {
            Console.WriteLine($"Searching for systems in {radius}ly distance from: {originSystem}");
            var systems = await client.SearchSystems(originSystem, radius);

            Console.WriteLine($"Found systems: {systems.Length}");
            return systems;
        }

        private static async Task WriteOutput(string originSystem, SystemDetails[] systemDetails)
        {
            await using var discoveredWriter = new StreamWriter($"discovered_{originSystem}.txt");
            await using var partialWriter = new StreamWriter($"partial_{originSystem}.txt");

            foreach (var sys in systemDetails.OrderBy(d => d.Ref?.Distance))
            {
                if (sys.IsNotFullyDiscovered)
                    await partialWriter.WriteLineAsync($"{sys.Ref} ({sys.Bodies?.Length.ToString() ?? "?"} discovered) => {sys.Url}");
                else if (sys.Id64.HasValue)
                    await discoveredWriter.WriteLineAsync(sys.Id64.ToString());
            }
            Console.WriteLine("Output files generated.");
        }

        private static int ParseRadius(string[] args)
        {
            return args.Length > 1 && int.TryParse(args[1], out var r) ? r : 50;
        }

        private static bool ShallPlotJourney(string[] args)
        {
            var enabled = new[] { "y", "1", "t", "true" };
            return args.Length > 2 && enabled.Contains(args[2], StringComparer.OrdinalIgnoreCase);
        }
    }
}
