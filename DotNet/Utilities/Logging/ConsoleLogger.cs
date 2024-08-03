using Microsoft.Extensions.Logging;

namespace Utilities.Logging;

public class ConsoleLogger : ILogger
{

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var log = $"{eventId.ToString()}|{formatter(state, exception)}";
        switch (logLevel)
        {
            case LogLevel.Trace:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(log);
                Console.ResetColor();
                break;
            case LogLevel.Debug:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(log);
                Console.ResetColor();
                break;
            case LogLevel.Information:
                Console.WriteLine(log);
                break;
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(log);
                Console.ResetColor();
                break;
            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(log);
                Console.ResetColor();
                break;
            case LogLevel.Critical:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(log);
                Console.ResetColor();
                break;
            case LogLevel.None:
                Console.WriteLine(log);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
    
    IDisposable? ILogger.BeginScope<TState>(TState state) => _logScopeHandler.BeginScope(state);

    private readonly LogScopeHandler _logScopeHandler = new();
    public bool IsEnabled(LogLevel logLevel) => true;
}