using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EdsmScanner
{
    internal class SystemDetailsResolver
    {
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
            try
            {
                return await _client.GetDetails(sys);
            }
            finally
            {
                Console.Write($"\r{Interlocked.Increment(ref _totals)}");
            }
        }

        public async Task<SystemDetails[]> GetSystemDetails()
        {
            _totals = 0;
            Console.WriteLine("Getting systems details...");
            var systemDetails = await Task.WhenAll(_systems.Select(GetDetails));
            Console.WriteLine();
            return systemDetails;
        }
    }
}