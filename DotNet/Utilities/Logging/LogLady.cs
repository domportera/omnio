using Microsoft.Extensions.Logging;

namespace Utilities.Logging;

public static class LogLady
{
    public static void AddLogger(ILogger logger) => Loggers.Add(logger);
    public static void AddLogger<T>() where T : ILogger, new() => Loggers.Add(new T());

    public static void RemoveLogger(ILogger logger) => Loggers.Remove(logger);
    
    private static readonly List<ILogger> Loggers = new();
    
    public static void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        foreach (var logger in Loggers)
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public static void Info(string message)
    {
        foreach (var logger in Loggers)
        {
            logger.LogInformation(message);
        }
    }

    public static void Error(Exception e)
    {
        Error(string.Empty, e);
    }
    
    public static void Error(string message, Exception? exception = null)
    {
        foreach (var logger in Loggers)
        {
            logger.LogError(message, exception);
        }
    }
    
    public static void Warning(string message)
    {
        foreach (var logger in Loggers)
        {
            logger.LogWarning(message);
        }
    }
    
    public static void Critical(Exception e)
    {
        Critical(string.Empty, e);
    }
    
    public static void Critical(string message, Exception? exception = null)
    {
        foreach (var logger in Loggers)
        {
            logger.LogCritical(message, exception);
        }
    }
    
    public static void Trace(string message)
    {
        foreach (var logger in Loggers)
        {
            logger.LogTrace(message);
        }
    }

    public static void Debug(string message)
    {
        foreach (var logger in Loggers)
        {
            logger.LogDebug(message);
        }
    }

    public static void RemoveAllLoggers() => Loggers.Clear();
}