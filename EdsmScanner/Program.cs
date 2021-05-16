using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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
        static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Option<int>(new []{"--scan-radius","-r"},50,"Scan radius in ly (default: 50)"),
                new Option<bool>(new []{"--plot-journey","-p"},false,"Plot journey (default: false)"),
                new Option<bool>(new []{"--include-bodies","-b"},false,"Include bodies in systems.txt (default: false)"),
                new Option<TimeSpan>(new []{"--cache-duration"},TimeSpan.FromMinutes(30),"Duration on how long system details are cached (default: 00:30:00)"),
            };
            cmd.Description = "Edsm Scanner";
            cmd.AddArgument(new Argument<string>("origin-system") { Description = "Origin system name" });
            cmd.Handler = CommandHandler.Create<string, int, bool, bool, TimeSpan>(Main);
            return await cmd.InvokeAsync(args);
        }

        static async Task Main(string originSystem, int scanRadius, bool plotJourney, bool includeBodies, TimeSpan cacheDuration)
        {
            using var client = new EdsmClient(new SystemCache(cacheDuration));
            var foundSystems = await new SystemResolver(client).ResolveSystemsAround(originSystem, scanRadius);

            var filteredSystems = new SystemFilter().FindPartiallyDiscovered(foundSystems);

            var remainingSystems = foundSystems.Except(filteredSystems).ToArray();
            await new VisitedSystemIdsWriter().WriteVisitedSystems(originSystem, remainingSystems);

            var orderedPartialSystems = new SystemOrderer(filteredSystems, plotJourney).Order();
            await new SystemListWriter().WriteSystemList(originSystem, orderedPartialSystems, includeBodies, plotJourney);
        }
    }
}
