namespace Darp.Utils.TestRail;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using Models;
using SourceGenerationContext = Models.SourceGenerationContext;

/// <summary> The implementation of a TestRail service </summary>
/// <param name="factoryState"> A state used when getting the HttpClient </param>
/// <param name="clientFactory"> A function for providing a HttpClient </param>
/// <param name="logger"> An optional logger </param>
/// <typeparam name="TState"> The type of the state </typeparam>
public sealed class TestRailService<TState>(TState factoryState, Func<TState, HttpClient> clientFactory, ILogger logger)
    : ITestRailService
{
    private readonly TState _factoryState = factoryState;
    private readonly Func<TState, HttpClient> _clientFactory = clientFactory;
    private readonly ILogger _logger = logger;

    /// <inheritdoc />
    public async Task<TResponse> GetAsync<TResponse>(
        string path,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage request = CreateRequestMessage(path, HttpMethod.Get);
        return await SendAsync(request, responseTypeInfo, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TResponse> GetAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage request = CreateRequestMessage(path, HttpMethod.Get);
        request.Content = JsonContent.Create(body, requestTypeInfo);
        return await SendAsync(request, responseTypeInfo, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TResponse> PostAsync<TResponse>(
        string path,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage request = CreateRequestMessage(path, HttpMethod.Post);
        request.Content = JsonContent.Create(Unit.Default, SourceGenerationContext.CustomOptions.Unit);
        return await SendAsync(request, responseTypeInfo, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage request = CreateRequestMessage(path, HttpMethod.Post);
        request.Content = JsonContent.Create(body, requestTypeInfo);
        return await SendAsync(request, responseTypeInfo, cancellationToken).ConfigureAwait(false);
    }

    private static HttpRequestMessage CreateRequestMessage(string path, HttpMethod method)
    {
        path = path.TrimStart('/');
        return new HttpRequestMessage
        {
            RequestUri = new Uri($"index.php?/api/v2/{path}", UriKind.Relative),
            Method = method,
        };
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpRequestMessage requestMessage,
        JsonTypeInfo<TResponse> jsonTypeInfo,
        CancellationToken cancellationToken
    )
    {
        HttpResponseMessage? responseMessage = null;
        try
        {
            HttpClient client = _clientFactory(_factoryState);
            responseMessage = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();

            TResponse? content = await responseMessage
                .Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken)
                .ConfigureAwait(false);
            return content ?? throw new JsonException("Deserialized content is null but must not be");
        }
        catch (Exception e) when (e is not HttpRequestException)
        {
            await _logger.LogRequestErrorStatusAsync(e, requestMessage, responseMessage).ConfigureAwait(false);
            throw;
        }
    }
}
