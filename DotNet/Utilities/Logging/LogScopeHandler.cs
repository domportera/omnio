using System.Collections.Concurrent;

namespace Utilities.Logging;

public class LogScopeHandler
{
    public LogScopeHandler()
    {
        _onScopeDispose = OnScopeDispose;
    }
    
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        if (!_scopePool.TryPop(out var scope))
        {
            scope = new LogScope();
            scope.Disposed += _onScopeDispose;
        }

        _scopes.Push(scope);
        
        var stateStr = state.ToString() ?? "Unspecified";
        ScopeBegan?.Invoke(stateStr);
        
        return scope;
    }

    private void OnScopeDispose(LogScope scope)
    {
        _scopes.TryPop(out _);
        _scopePool.Push(scope);
        ScopeEnded?.Invoke();
    }

    public event Action<string>? ScopeBegan;
    public event Action? ScopeEnded;
    private readonly Action<LogScope> _onScopeDispose;
    private readonly ConcurrentStack<LogScope> _scopes = new();
    private readonly ConcurrentStack<LogScope> _scopePool = new();

    private class LogScope : IDisposable
    {
        public event Action<LogScope> Disposed = null!;
        
        public void Dispose()
        {
            Disposed!(this);
        }
    }
}