namespace Darp.Utils.TestRail.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using VerifyTests.Http;

public static class VerifyHelper
{
    public static SettingsTask VerifyCalls(MockHttpClient client)
    {
        return Verify(new { client.Calls }).UseDirectory("Snapshots").IgnoreMember("Content-Length");
    }

    public static SettingsTask VerifyCalls<T>(MockHttpClient client, T response)
    {
        return Verify(new { client.Calls, Response = response })
            .UseDirectory("Snapshots")
            .IgnoreMember("Content-Length");
    }

    public static MockHttpClient CreateService<TRequest, TResponse>(
        Func<TRequest, TResponse> func,
        out ITestRailService service
    )
    {
        var client = new MockHttpClient(responseBuilder: request =>
        {
            ArgumentNullException.ThrowIfNull(request.Content);
            // Read as string to avoid disposal when using ReadFromJsonAsync
            var requestContentText = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            TRequest? requestContent = JsonSerializer.Deserialize<TRequest>(
                requestContentText,
                SourceGenerationContext.CustomOptions
            );
            TResponse responseValue = func(requestContent!);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(responseValue, options: SourceGenerationContext.CustomOptions),
            };

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
            {
                response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return response;
        })
        {
            BaseAddress = new Uri("https://fake.com"),
        };
        service = new TestRailService<MockHttpClient>(client, c => c, NullLogger.Instance);
        return client;
    }

    public static MockHttpClient CreateService(string response, out ITestRailService service)
    {
        var client = new MockHttpClient(content: response, mediaType: "application/json")
        {
            BaseAddress = new Uri("https://fake.com"),
        };
        service = new TestRailService<MockHttpClient>(client, c => c, NullLogger.Instance);
        return client;
    }
}
