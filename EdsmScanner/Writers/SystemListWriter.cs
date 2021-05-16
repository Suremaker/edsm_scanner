using System;
using System.IO;
using System.Threading.Tasks;
using EdsmScanner.Models;

namespace EdsmScanner.Writers
{
    internal class SystemListWriter
    {
        public async Task WriteSystemList(string originSystem, SystemDetails[] systems, bool journeyPlotted)
        {
            var path = PathSanitizer.SanitizePath($"systems_{originSystem}.txt");
            await using var writer = new StreamWriter(path);

            await writer.WriteLineAsync(journeyPlotted ? $"# distances calculated to previous system, starting from: {originSystem}" : $"# distances calculated to origin system: {originSystem}");

            foreach (var sys in systems)
            {
                var distance = journeyPlotted ? sys.PlottedDistance : sys.Ref.Distance;
                await writer.WriteLineAsync($"{sys.Ref.Name} [{distance:F2}ly] ({sys.Ref.BodyCount?.ToString() ?? "?"} bodies / {sys.Bodies?.Length.ToString() ?? "?"} discovered) => {sys.Url}");
            }

            Console.WriteLine($"Generated {path} with {systems.Length} systems.");
        }
    }
}