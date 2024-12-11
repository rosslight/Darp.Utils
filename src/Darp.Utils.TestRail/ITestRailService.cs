namespace Darp.Utils.TestRail;

using System.Text.Json.Serialization.Metadata;

/// <summary> The abstract definition of a TestRail service </summary>
public interface ITestRailService
{
    /// <summary> Execute a <c>GET</c> request to the TestRail instance </summary>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="responseTypeInfo"> The JsonTypeInfo used to control the serialization behavior of the response </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    public Task<TResponse> GetAsync<TResponse>(
        string path,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    );

    /// <summary> Execute a <c>GET</c> request with a request body to the TestRail instance </summary>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="body"> The request body which will be serialized as json </param>
    /// <param name="requestTypeInfo"> The JsonTypeInfo used to control the serialization behavior of the request </param>
    /// <param name="responseTypeInfo"> The JsonTypeInfo used to control the serialization behavior of the response </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TRequest"> The type of the request </typeparam>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    public Task<TResponse> GetAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    );

    /// <summary> Execute a <c>POST</c> request to the TestRail instance </summary>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="responseTypeInfo"> The JsonTypeInfo used to control the serialization behavior of the response </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    public Task<TResponse> PostAsync<TResponse>(
        string path,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    );

    /// <summary> Execute a <c>POST</c> request with a request body to the TestRail instance </summary>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="body"> The request body which will be serialized as json </param>
    /// <param name="requestTypeInfo"> The JsonTypeInfo used to control the serialization behavior of the request </param>
    /// <param name="responseTypeInfo"> The JsonTypeInfo used to control the serialization behavior of the response </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TRequest"> The type of the request </typeparam>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    public Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken
    );
}
