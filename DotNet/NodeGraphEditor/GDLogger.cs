using System;
using Godot;
using Microsoft.Extensions.Logging;
using Utilities.Logging;

namespace NodeGraphEditor;

public class GDLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var log = $"{eventId.ToString()}|{formatter(state, exception)}";
        switch (logLevel)
        {
            // todo - GD.PrintRich w/ bbcode
            case LogLevel.Trace:
                GD.PrintRich($"[fgcolor=darkgray]{log}[/]");
                break;
            case LogLevel.Debug:
                GD.PrintRich($"[fgcolor=gray]{log}[/]");
                break;
            case LogLevel.Information:
                GD.Print(log);
                break;
            case LogLevel.Warning:
                GD.PrintRich($"[fgcolor=yellow]{log}[/]");
                break;
            case LogLevel.Error:
                GD.PrintErr(log);
                break;
            case LogLevel.Critical:
                GD.PrintErr(log);
                break;
            case LogLevel.None:
                GD.Print(log);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
    
    public bool IsEnabled(LogLevel logLevel) => true;
    
    private readonly LogScopeHandler _logScopeHandler = new();
    IDisposable? ILogger.BeginScope<TState>(TState state) => _logScopeHandler.BeginScope(state);
}