namespace Darp.Utils.TestRail;

using System.Net;
using Microsoft.Extensions.Logging;

internal static partial class LoggerDefinitions
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Could not get request from testrail from {HttpRequestUri} because of {HttpResponseStatusCode}"
    )]
    private static partial void LogRequestError(
        this ILogger logger,
        Exception exception,
        string? httpRequestUri,
        HttpStatusCode httpResponseStatusCode
    );

    public static async Task LogRequestErrorStatusAsync(
        this ILogger logger,
        Exception exception,
        HttpRequestMessage requestMessage,
        HttpResponseMessage? responseMessage
    )
    {
        var stateDict = new Dictionary<string, object?>();
        if (requestMessage.Content is not null)
        {
            stateDict["HttpRequestContent"] = await requestMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        if (responseMessage is not null)
        {
            stateDict["HttpResponseContent"] = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        using (logger.BeginScope(stateDict))
        {
            logger.LogRequestError(
                exception,
                requestMessage.RequestUri?.ToString(),
                responseMessage?.StatusCode ?? default
            );
        }
    }
}
