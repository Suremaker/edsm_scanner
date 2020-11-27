using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisitedStarCacheMerger
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("VisitedStarCacheMerger.exe [path to VisitedStarsCache.dat] [path to system ids txt]");
                return;
            }

            var cachePath = GetFilePath(args, 0, "[path to VisitedStarsCache.dat]");
            var idsPath = GetFilePath(args, 1, "[path to system ids txt]");

            var cache = ReadCache(cachePath);
            var ids = ReadIdsToMerge(idsPath);

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

        private static IEnumerable<long> ReadIdsToMerge(string path)
        {
            Console.WriteLine($"Reading system ids from: {path}");
            return File.ReadAllLines(path).Select(long.Parse).ToArray();
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
