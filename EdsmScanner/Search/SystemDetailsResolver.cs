using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdsmScanner.Clients;
using EdsmScanner.Models;

namespace EdsmScanner.Search
{
    internal class SystemDetailsResolver
    {
        private readonly SemaphoreSlim _throttler = new SemaphoreSlim(20);
        private readonly EdsmClient _client;
        private readonly SystemRef[] _systems;
        private int _totals;
        public SystemDetailsResolver(EdsmClient client, SystemRef[] systems)
        {
            _client = client;
            _systems = systems;
        }

        private async Task<SystemDetails> GetDetails(SystemRef sys)
        {
            await _throttler.WaitAsync();
            try
            {
                return await _client.GetDetails(sys);
            }
            finally
            {
                Console.Write($"\r{Interlocked.Increment(ref _totals)}");
                _throttler.Release();
            }
        }

        public async Task<SystemDetails[]> GetSystemDetails()
        {
            _totals = 0;
            Console.WriteLine("Getting systems details...");
            try
            {
                return await Task.WhenAll(_systems.Select(GetDetails));
            }
            finally
            {
                Console.WriteLine();
            }
        }
    }
}