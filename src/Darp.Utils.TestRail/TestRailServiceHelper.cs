namespace Darp.Utils.TestRail;

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

file readonly record struct TestRailInstanceState(string Instance, string Username, string Password);

/// <summary> A class defining helper methods </summary>
public static partial class TestRailService
{
    /// <summary> Create a new TestRail service using username and password for authentication </summary>
    /// <param name="instance"> The instance url </param>
    /// <param name="username"> The username </param>
    /// <param name="password"> The password </param>
    /// <param name="logger"> An optional logger </param>
    /// <returns> A TestRail service </returns>
    public static ITestRailService Create(string instance, string username, string password, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        return new TestRailService<TestRailInstanceState>(
            new TestRailInstanceState(instance, username, password),
            state =>
            {
                var client = new HttpClient { BaseAddress = new Uri(state.Instance) };
                var authBytes = Encoding.UTF8.GetBytes($"{state.Username}:{state.Password}");
                var base64Authorization = Convert.ToBase64String(authBytes);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    base64Authorization
                );
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client;
            },
            logger
        );
    }
}
