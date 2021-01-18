using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace EdsmScanner
{
    class EdsmClient : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly HttpClient _client;

        public EdsmClient()
        {
            var collection = new ServiceCollection();
            collection
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
            var details = await _client.GetFromJsonAsync<SystemDetails>($"api-system-v1/bodies?systemName={Uri.EscapeDataString(sys.Name)}")
                          ?? throw new InvalidOperationException($"Unable to get details for {sys}");
            details.Ref = sys;
            return details;
        }

        public async Task<SystemRef[]> SearchSystems(string originSystem, int radius)
        {
            return await _client.GetFromJsonAsync<SystemRef[]>($"api-v1/sphere-systems?systemName={Uri.EscapeDataString(originSystem)}&radius={radius}&showCoordinates=1")
                   ?? throw new InvalidOperationException("Unable to fetch systems");
        }
    }
}