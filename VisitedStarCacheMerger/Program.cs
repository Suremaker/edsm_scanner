using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VisitedStarCacheMerger
{
    class Program
    {
        private static readonly string TempCachePath = $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}.cache";
        private static int _totalIdsLoaded = 0;

        static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("VisitedStarCacheMerger.exe [path to VisitedStarsCache.dat] [path to system ids/names txt or another Cache.dat]");
                return;
            }

            var cachePath = GetFilePath(args, 0, "[path to VisitedStarsCache.dat]");
            var idsPath = GetFilePath(args, 1, "[path to system ids/names txt or another Cache.dat]");

            var cache = ReadCache(cachePath);
            if (Path.GetExtension(idsPath).Equals(".dat"))
            {
                var cache2 = ReadCache(idsPath);
                Console.WriteLine("Merging cache files...");
                cache.MergeCaches(cache2);
            }
            else
            {
                var ids = await ReadIdsToMerge(idsPath);
                Console.WriteLine("Merging system IDs with Cache...");
                cache.MergeSystemIds(ids);
            }

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
            Console.WriteLine($"Written systems: {cache.Count}");
        }

        private static async Task<long[]> ReadIdsToMerge(string path)
        {
            using var client = new EdsmClient();
            Console.WriteLine($"Reading system ids from: {path}");

            var ids = await Task.WhenAll(File.ReadAllLines(path)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => ParseId(client, line)));
            Console.WriteLine();
            var failed = ids.Where(x => !x.Id64.HasValue).ToArray();
            if (failed.Any())
            {
                Console.WriteLine("Failed systems:");
                foreach (var sys in failed)
                    Console.WriteLine($"{sys.Name}: {sys.Exception?.Message}");
            }

            return ids.Where(i => i.Id64.HasValue).Select(x => x.Id64.GetValueOrDefault()).ToArray();
        }

        record SysIdLine(string Name, long? Id64 = null, Exception? Exception = null);
        private static async Task<SysIdLine> ParseId(EdsmClient client, string line)
        {
            try
            {
                if (long.TryParse(line, out var id))
                    return new(line, id);
                try
                {
                    return new SysIdLine(line, await MapNameToId64(client, line));
                }
                catch (Exception ex)
                {
                    return new SysIdLine(line, null, ex);
                }
            }
            finally
            {
                Console.Write($"\rSystems read: {Interlocked.Increment(ref _totalIdsLoaded)}");
            }
        }

        private static async Task<long> MapNameToId64(EdsmClient client, string line)
        {
            if (!Directory.Exists(TempCachePath))
                Directory.CreateDirectory(TempCachePath);

            var systemName = line.ToLowerInvariant().Trim();
            var path = $"{TempCachePath}{Path.DirectorySeparatorChar}{string.Join("_", systemName.Split(Path.GetInvalidFileNameChars()))}";
            if (File.Exists(path) && long.TryParse(await File.ReadAllTextAsync(path), out var id64))
                return id64;
            var result = await client.GetId64(systemName);
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
            var cache = Cache.Read(input);
            Console.WriteLine($"Systems read: {cache.Count}");
            return cache;
        }
    }
}
