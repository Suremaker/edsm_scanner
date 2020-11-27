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
                    .WaitAndRetryAsync(15, _ => TimeSpan.FromSeconds(1)))
                .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(20, int.MaxValue));
            _provider = collection.BuildServiceProvider();
            _client = _provider.GetService<IHttpClientFactory>().CreateClient("edsm");
        }

        public void Dispose()
        {
            _provider?.Dispose();
        }

        public async Task<SystemDetails> GetDetails(SystemRef sys)
        {
            var details = await _client.GetFromJsonAsync<SystemDetails>($"api-system-v1/bodies?systemName={Uri.EscapeDataString(sys.Name)}");
            details.Ref = sys;
            return details;
        }

        public async Task<SystemRef[]> SearchSystems(string originSystem, int radius) => await _client.GetFromJsonAsync<SystemRef[]>($"api-v1/sphere-systems?systemName={Uri.EscapeDataString(originSystem)}&radius={radius}");
    }
}