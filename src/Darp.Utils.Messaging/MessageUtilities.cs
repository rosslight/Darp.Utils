namespace Darp.Utils.Messaging;

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1062 // Validate arguments of public methods

/// <summary> Utility methods for messaging </summary>
public static class MessageUtilities
{
    /// <summary> Fire and forget a value task </summary>
    /// <param name="valueTask"> The task </param>
    /// <param name="handleException"> The callback to handle the exception </param>
    /// <seealso href="https://github.com/TheCodeTraveler/AsyncAwaitBestPractices/blob/36b74057472055c4aad0864c1967a50ccdfc0fe9/src/AsyncAwaitBestPractices/SafeFireAndForgetExtensions.shared.cs#L106"/>
    public static async void SafeFireForget(ValueTask valueTask, Action<Exception>? handleException = null)
    {
        try
        {
            await valueTask.ConfigureAwait(false);
        }
        catch (Exception e) when (handleException is not null)
        {
            handleException(e);
        }
    }
}
