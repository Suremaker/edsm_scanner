using System;
using System.Linq;
using EdsmScanner.Models;

namespace EdsmScanner.Search
{
    internal class SystemFilter
    {
        public SystemDetails[] FindPartiallyDiscovered(SystemDetails[] systems)
        {
            Console.WriteLine("Searching for partially discovered systems...");
            var result = systems.Where(s => s.IsNotFullyDiscovered).ToArray();
            Console.WriteLine($"  Found {result.Length} systems");
            return result;
        }
    }
}