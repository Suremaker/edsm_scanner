using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EdsmScanner.Clients;
using EdsmScanner.Models;
using EdsmScanner.Plotting;
using EdsmScanner.Search;
using EdsmScanner.Writers;

namespace EdsmScanner
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var cmd = new RootCommand
            {
                new Option<int>(new []{"--scan-radius","-r"},50,"Scan radius in ly (default: 50)"),
                new Option<bool>(new []{"--plot-journey","-p"},false,"Plot journey (default: false)"),
                new Option<bool>(new []{"--include-bodies","-b"},false,"Include bodies in systems.txt (default: false)"),
                new Option<TimeSpan>(new []{"--cache-duration"},TimeSpan.FromMinutes(30),"Duration on how long system details are cached (default: 00:30:00)"),
                new Option<string[]>(new []{"--filter-body","-fb"},Array.Empty<string>,"Body filter(s) written in form on LINQ expression like: IsScoopable==true. When applied, only the systems with at least one matching body will be returned."),
                new Option<string[]>(new []{"--filter-system","-fs"},Array.Empty<string>,"System filter(s) written in form on LINQ expression like: StarCount > 1.")
            };
            cmd.Description = "Edsm Scanner";
            cmd.AddArgument(new Argument<string>("origin-system") { Description = "Origin system name" });
            cmd.Handler = CommandHandler.Create<string, int, bool, bool, TimeSpan, string[], string[]>(Scan);


            var helpCmd = new Command("help", "Displays help");
            helpCmd.AddCommand(new Command("usage", "Displays usage") { Handler = CommandHandler.Create(HelpUsage) });
            helpCmd.AddCommand(new Command("filters", "Displays filters usage") { Handler = CommandHandler.Create(FiltersUsage) });

            cmd.AddCommand(helpCmd);
            return await cmd.InvokeAsync(args);
        }

        private static void FiltersUsage()
        {
            Console.WriteLine("--filter-system option allows to filter systems that match the criteria, where the following attributes can be used in the filter:");
            Console.WriteLine();
            ListQueryableProperties(typeof(SystemDetails));
            Console.WriteLine();

            Console.WriteLine("--filter-body option allows to filter systems that have at least one body matching the criteria, where the following attributes can be used in the filter:");
            Console.WriteLine();
            ListQueryableProperties(typeof(SystemBody));
            Console.WriteLine();
        }

        private static void ListQueryableProperties(Type type)
        {
            foreach (var property in type.GetProperties()
                .Where(p => p.GetCustomAttributes<QueryableAttribute>().Any()).OrderBy(x => x.Name))
            {
                var propType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                Console.WriteLine($"  {property.Name} - {propType.Name}");
            }
        }

        private static void HelpUsage()
        {
            Console.WriteLine("EdsmScanner allows to scan the nearby systems in order to find the interesting features.");
            Console.WriteLine();
            Console.WriteLine("Sample usages:");
            Console.WriteLine();
            Console.WriteLine("EdsmScanner Sol\n  Scans the systems around Sol and generates systems_Sol.txt with list of them (ordered by distance)");
            Console.WriteLine();
            Console.WriteLine("EdsmScanner Sol -p\n  Plots the journey through the systems around Sol and generates systems_Sol.txt with list of them (ordered by journey steps)");
            Console.WriteLine();
            Console.WriteLine("EdsmScanner Sol -p -b -r 100\n  Scans the systems around Sol in range of 100ly, plots the journey around and includes all discovered bodies on the systems_Sol.txt");
            Console.WriteLine();
            Console.WriteLine($"EdsmScanner \"V970 Scorpii\" -p -fs \"{nameof(SystemDetails.IsFullyDiscovered)}==false\"\n  Scans the systems around V970 Scorpii and plots the journey around ones which are not fully discovered yet. The fully discovered systems are added to visited_V970 Scorpii.txt");
            Console.WriteLine();
            Console.WriteLine($"EdsmScanner \"V970 Scorpii\" -p -b -fs \"{nameof(SystemDetails.DiscoveredStars)}>1\" -fb \"{nameof(SystemBody.SurfacePressure)}>0\" \"{nameof(SystemBody.SurfacePressure)}<0.1\"\n  Scans the systems around V970 Scorpii and plots the journey around ones with multiple stars and having planets with thin atmosphere. All not matching systems are added to visited_V970 Scorpii.txt");
            Console.WriteLine();
            Console.WriteLine("EdsmScanner \"V970 Scorpii\" -fs \"false\"\n  Scans the systems around V970 Scorpii and includes all of them in visited_V970 Scorpii.txt");
            Console.WriteLine();
            Console.WriteLine("EdsmScanner Sol -b -fb \"RingTypes.Contains(\\\"Icy\\\")\" \"ReserveLevel==\\\"Pristine\\\"\" \"DistanceToArrival<1000\"\n  Scans the systems around Sol for bodies having pristine, icy rings, located in less than 1000ls from the main star");
            Console.WriteLine();
            Console.WriteLine("EdsmScanner help filters\n  Prints help for using filters");
            Console.WriteLine();
        }

        static async Task Scan(string originSystem, int scanRadius, bool plotJourney, bool includeBodies, TimeSpan cacheDuration, string[] filterSystem, string[] filterBody)
        {
            using var client = new EdsmClient(new SystemCache(cacheDuration));
            var foundSystems = await new SystemResolver(client).ResolveSystemsAround(originSystem, scanRadius);

            var filteredSystems = new SystemFilter(filterSystem, filterBody).Filter(foundSystems);

            var remainingSystems = foundSystems.Except(filteredSystems).ToArray();
            await new VisitedSystemIdsWriter().WriteVisitedSystems(originSystem, remainingSystems);

            var orderedPartialSystems = new SystemOrderer(filteredSystems, plotJourney).Order();
            await new SystemListWriter().WriteSystemList(originSystem, orderedPartialSystems, includeBodies, plotJourney);
        }
    }
}
