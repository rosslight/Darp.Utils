namespace Darp.Utils.TestRail.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using VerifyTests.Http;

public static class VerifyHelper
{
    public static SettingsTask VerifyCalls<T>(MockHttpClient client, T response)
    {
        return Verify(new { client.Calls, Response = response })
            .UseDirectory("Snapshots")
            .IgnoreMember("Content-Length");
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
