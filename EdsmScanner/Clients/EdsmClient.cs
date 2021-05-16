using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using EdsmScanner.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace EdsmScanner.Clients
{
    class EdsmClient : IDisposable
    {
        private readonly SystemCache _cache;
        private readonly ServiceProvider _provider;
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public EdsmClient(SystemCache cache)
        {
            _cache = cache;
            var collection = new ServiceCollection();
            collection
                .AddLogging(x => x.AddDebug())
                .AddHttpClient("edsm", x => x.BaseAddress = new Uri("https://www.edsm.net/"))
                .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
                    .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(45, _ => TimeSpan.FromSeconds(3)));
            _provider = collection.BuildServiceProvider();
            _client = _provider.GetRequiredService<IHttpClientFactory>().CreateClient("edsm");
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        public async Task<SystemDetails> GetDetails(SystemRef sys)
        {
            try
            {
                var details = (await QueryWithCache(sys.Name))!;
                details.Ref = sys;
                return details;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to get details for: {sys}", ex);
            }
        }

        private async Task<SystemDetails?> QueryWithCache(string systemName)
        {
            var body = _cache.TryRetrieve(systemName);
            if (string.IsNullOrWhiteSpace(body))
                body = _cache.Update(systemName, await _client.GetStringAsync($"api-system-v1/bodies?systemName={Uri.EscapeDataString(systemName)}"));

            return JsonSerializer.Deserialize<SystemDetails>(body, JsonSerializerOptions);
        }

        public async Task<SystemRef[]> SearchSystems(string originSystem, int radius)
        {
            try
            {
                return (await _client.GetFromJsonAsync<SystemRef[]>($"api-v1/sphere-systems?systemName={Uri.EscapeDataString(originSystem)}&radius={radius}&showCoordinates=1"))!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to fetch systems for: {originSystem}", ex);
            }
        }
    }
}