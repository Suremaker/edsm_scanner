using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VisitedStarCacheMerger
{
    class Program
    {
        private static readonly string TempCachePath = $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}.cache";

        static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("VisitedStarCacheMerger.exe [path to VisitedStarsCache.dat] [path to system ids/names txt]");
                return;
            }

            var cachePath = GetFilePath(args, 0, "[path to VisitedStarsCache.dat]");
            var idsPath = GetFilePath(args, 1, "[path to system ids/names txt]");

            var cache = ReadCache(cachePath);
            var ids = await ReadIdsToMerge(idsPath).ToArrayAsync();

            Console.WriteLine("Merging...");
            cache.MergeSystemIds(ids);

            Save(cache, cachePath);
        }

        private static void Save(Cache cache, string cachePath)
        {
            var bakPath = $"{cachePath}.bak_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}";
            Console.WriteLine($"Backing up current cache: {bakPath}");
            File.Move(cachePath, bakPath);

            Console.WriteLine($"Writing cache: {cachePath}");
            using var output = new BinaryWriter(File.OpenWrite(cachePath));
            cache.Write(output);
        }

        private static async IAsyncEnumerable<long> ReadIdsToMerge(string path)
        {
            using var client = new EdsmClient();
            Console.WriteLine($"Reading system ids from: {path}");
            foreach (var line in await File.ReadAllLinesAsync(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (long.TryParse(line, out var id))
                    yield return id;
                else
                {
                    Console.WriteLine($"  Obtaining id for: {line}");
                    var sysId = await MapNameToId64(client, line);
                    if (sysId != null)
                        yield return sysId.Value;
                    else
                        Console.WriteLine("    System Id not found.");
                }
            }
        }

        private static async Task<long?> MapNameToId64(EdsmClient client, string line)
        {
            if (!Directory.Exists(TempCachePath))
                Directory.CreateDirectory(TempCachePath);

            var systemName = line.ToLowerInvariant().Trim();
            var path = $"{TempCachePath}{Path.DirectorySeparatorChar}{string.Join("_", systemName.Split(Path.GetInvalidFileNameChars()))}";
            if (File.Exists(path) && long.TryParse(await File.ReadAllTextAsync(path), out var id64))
                return id64;
            var result = await client.GetId64(systemName);
            if (result != null)
                await File.WriteAllTextAsync(path, result.ToString());
            return result;
        }

        private static string GetFilePath(string[] args, int index, string name)
        {
            if (args.Length <= index || !File.Exists(args[index]))
                throw new InvalidOperationException($"{name} is not specified or file does not exists");
            return args[index];
        }

        private static Cache ReadCache(string path)
        {
            Console.WriteLine($"Reading cache from: {path}");
            using var input = new BinaryReader(File.OpenRead(path));
            return Cache.Read(input);
        }
    }
}
