using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EdsmScanner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("EdsmScanner.exe [origin-system-name] [scan-radius]");
                Console.WriteLine("scan-radius is optional");
                return;
            }

            var originSystem = args[0];
            var radius = ParseRadius(args);

            using var client = new EdsmClient();
            var systems = await SearchSystems(client, originSystem, radius);
            var systemDetails = await new SystemDetailsResolver(client, systems).GetSystemDetails();
            await WriteOutput(originSystem, systemDetails);
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
        }

        private static int ParseRadius(string[] args)
        {
            return args.Length > 1 && int.TryParse(args[1], out var r) ? r : 50;
        }
    }
}
