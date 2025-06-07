namespace Darp.Utils.CodeMirror;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MirrorSharp.Advanced;

internal sealed partial class ExceptionLogger(ILogger<ExceptionLogger>? logger) : IExceptionLogger
{
    private readonly ILogger<ExceptionLogger> _logger = logger ?? NullLogger<ExceptionLogger>.Instance;

    public void LogException(Exception exception, IWorkSession session)
    {
        using IDisposable? _ = _logger.BeginScope(
            new Dictionary<string, object>
            {
                [nameof(IWorkSession.IsRoslyn)] = session.IsRoslyn,
                [nameof(IWorkSession.LanguageName)] = session.LanguageName,
                [nameof(IWorkSession.ExtensionData)] = session.ExtensionData,
            }
        );
        LogException(exception, exception.Message);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "CodeMirror with exception: {Message}")]
    private partial void LogException(Exception e, string message);
}
