using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
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
        private readonly SemaphoreSlim _sem = new(10);

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

        public async Task<long> GetId64(string systemName)
        {
            await _sem.WaitAsync();
            try
            {
                using var result = await _client.GetAsync($"api-v1/system?systemName={Uri.EscapeDataString(systemName)}&showId=1");
                if (!result.IsSuccessStatusCode)
                    throw new InvalidOperationException($"EDSM failed with: {(int)result.StatusCode}");
                var content = await result.Content.ReadAsStringAsync();
                if (content.StartsWith("["))
                    throw new InvalidOperationException("System ID not found");
                return (JsonSerializer.Deserialize<SysDetails>(content, JsonSerializerOptions))?.Id64
                       ?? throw new InvalidOperationException("System ID not found");
            }
            finally
            {
                _sem.Release();
            }
        }

        private record SysDetails
        {
            public long? Id64 { get; init; }
        }
    }
}