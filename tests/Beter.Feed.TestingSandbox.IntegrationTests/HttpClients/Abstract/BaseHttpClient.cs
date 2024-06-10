using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Exceptions;
using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Extensions;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract
{
    public class BaseHttpClient
    {
        private readonly HttpClient _httpClient;

        public BaseHttpClient(Uri baseUrl)
        {
            _httpClient = new HttpClient() { BaseAddress = baseUrl };
        }

        protected async Task<string> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryPolicy = GetRetryPolicy();
            var circuitBreakerPolicy = GetCircuitBreakerPolicy();

            var response = await retryPolicy
                .WrapAsync(circuitBreakerPolicy)
                .ExecuteAsync(async () => await _httpClient.SendAsync(request, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                ThrowBadRequestException(request.RequestUri);
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async Task WaitForServiceReadiness()
        {
            await _httpClient.GetAsyncAndWaitForStatusCode("/health", HttpStatusCode.OK);
        }

        protected static StringContent MapToContent<T>(T data) where T : class
        {
            return new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        }

        private static void ThrowBadRequestException(Uri requestUri) =>
            throw new BadRequestException($"Unknown error occured during processing uri: {requestUri}.");

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
