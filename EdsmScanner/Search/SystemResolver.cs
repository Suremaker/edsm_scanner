using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdsmScanner.Clients;
using EdsmScanner.Models;

namespace EdsmScanner.Search
{
    internal class SystemResolver
    {
        private readonly SemaphoreSlim _throttler = new SemaphoreSlim(20);
        private readonly EdsmClient _client;

        public SystemResolver(EdsmClient client)
        {
            _client = client;
        }

        public async Task<SystemDetails[]> ResolveSystemsAround(string originSystem, int radius)
        {
            var systems = await SearchForSystems(originSystem, radius);
            return await GetSystemsDetails(systems);
        }

        private async Task<SystemDetails[]> GetSystemsDetails(SystemRef[] systems)
        {
            Console.WriteLine("Getting systems details...");
            var notifier = new ProgressNotifier($"  Scanned systems: {{0}}/{systems.Length}");
            try
            {
                return await Task.WhenAll(systems.Select(r => GetSystemDetails(r, notifier)));
            }
            finally
            {
                notifier.Finish();
            }
        }

        private async Task<SystemDetails> GetSystemDetails(SystemRef sys, ProgressNotifier notifier)
        {
            await _throttler.WaitAsync();
            try
            {
                return await _client.GetDetails(sys);
            }
            finally
            {
                notifier.NotifyIncrease();
                _throttler.Release();
            }
        }

        private async Task<SystemRef[]> SearchForSystems(string originSystem, int radius)
        {
            Console.WriteLine($"Searching for systems in {radius}ly distance from: {originSystem}");
            var systems = await _client.SearchSystems(originSystem, radius);
            Console.WriteLine($"  Found systems: {systems.Length}");
            return systems;
        }
    }
}