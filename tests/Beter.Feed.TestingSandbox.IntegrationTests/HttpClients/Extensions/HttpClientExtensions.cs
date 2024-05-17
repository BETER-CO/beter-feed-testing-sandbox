using Polly;
using System.Net;

namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task GetAsyncAndWaitForStatusCode(this HttpClient httpClient, string url, HttpStatusCode statusCode)
        {
            return Policy.Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode != statusCode)
                .WaitAndRetryAsync(60, _ => TimeSpan.FromMilliseconds(1000))
                .ExecuteAsync(async () =>
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.StatusCode != statusCode)
                    {
                        throw new HttpRequestException($"Expected status code {statusCode}, but received {response.StatusCode} for URL: {url}");
                    }

                    return response;
                });
        }
    }
}
