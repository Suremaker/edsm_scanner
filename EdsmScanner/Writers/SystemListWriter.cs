using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EdsmScanner.Models;

namespace EdsmScanner.Writers
{
    internal class SystemListWriter
    {
        public async Task WriteSystemList(string originSystem, SystemDetails[] systems, bool includeBodies, bool journeyPlotted)
        {
            var path = PathSanitizer.SanitizePath($"systems_{originSystem}.txt");
            await using var writer = new StreamWriter(path);

            await writer.WriteLineAsync(journeyPlotted ? $"# distances calculated to previous system, starting from: {originSystem}" : $"# distances calculated to origin system: {originSystem}");

            foreach (var sys in systems)
            {
                var distance = journeyPlotted ? sys.PlottedDistance : sys.Ref.Distance;
                await WriteSystem(writer, sys, distance);
                if (includeBodies)
                {
                    foreach (var body in sys.FilteredBodies.OrderBy(b => b.DistanceToArrival))
                    {
                        await WriteBody(writer, body);
                    }
                }
            }

            Console.WriteLine($"Generated {path} with {systems.Length} systems.");
        }

        private static async Task WriteSystem(StreamWriter writer, SystemDetails sys, decimal distance)
        {
            await writer.WriteLineAsync($"{sys.Ref.Name} [{distance:F2}ly] ({sys.Ref.BodyCount?.ToString() ?? "?"} bodies / {sys.DiscoveredBodies} discovered, of which {sys.DiscoveredStars} stars) => {sys.Url}");
        }

        private static async Task WriteBody(StreamWriter writer, SystemBody body)
        {
            if (body.Type.Equals("star", StringComparison.OrdinalIgnoreCase))
                await writer.WriteLineAsync($"  {body.Name}: {body.Type}/{body.SubType} distance:{body.DistanceToArrival}ls {(body.IsScoopable.GetValueOrDefault() ? "scoopable" : "non-scoopable")} age:{body.Age} luminosity:{body.Luminosity} spectralClass:{body.SpectralClass} mass:{body.SolarMasses} radius:{body.SolarRadius} temperature:{body.SurfaceTemperature}K reserve:{body.ReserveLevel} belts:{string.Join(",", body.Belts.Select(b => b.Type))} rings:{string.Join(",", body.Rings.Select(b => b.Type))}");
            else if (body.Type.Equals("planet", StringComparison.OrdinalIgnoreCase))
                await writer.WriteLineAsync($"  {body.Name}: {body.Type}/{body.SubType} distance:{body.DistanceToArrival}ls {(body.IsLandable.GetValueOrDefault() ? "landable" : "not-landable")} gravity:{body.Gravity}G earthMasses:{body.EarthMasses} temperature:{body.SurfaceTemperature}K pressure:{body.SurfacePressure} atmosphere:{body.AtmosphereType} volcanism:{body.VolcanismType} terraforming:{body.TerraformingState} reserve:{body.ReserveLevel} rings:{string.Join(",", body.Rings.Select(b => b.Type))}");
            else
                await writer.WriteLineAsync($"  {body.Name}: {body.Type}/{body.SubType} distance:{body.DistanceToArrival}ls");
        }
    }
}