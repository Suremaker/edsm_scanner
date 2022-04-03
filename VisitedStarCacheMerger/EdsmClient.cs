using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace VisitedStarCacheMerger
{
    class EdsmClient : IDisposable
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private readonly ServiceProvider _provider;

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

        public async Task<long?> GetId64(string systemName)
        {
            try
            {
                var sys = await _client.GetFromJsonAsync<SysDetails>($"api-system-v1/bodies?systemName={Uri.EscapeDataString(systemName)}", JsonSerializerOptions);
                return sys?.Id64;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to get details for: {systemName}", ex);
            }
        }

        private record SysDetails
        {
            public long? Id64 { get; init; }
        }
    }
}