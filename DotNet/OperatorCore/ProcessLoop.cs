using System.Diagnostics;

namespace OperatorCore;

// todo - multithreaded contexts if necessary
internal static class ProcessLoop
{
    static ProcessLoop()
    {
        Start();
    }

    public static void Add(GraphNodeLogic node)
    {
        if (IsRunning)
        {
            lock (NodeAddRemoveEvents)
                NodeAddRemoveEvents.Enqueue(new NodeAddRemoveEvent(node, true));
        }
        else
        {
            Nodes.Add(node);
        }
    }

    public static void Remove(GraphNodeLogic node)
    {
        if (IsRunning)
        {
            lock (NodeAddRemoveEvents)
                NodeAddRemoveEvents.Enqueue(new NodeAddRemoveEvent(node, false));
        }
        else
        {
            Nodes.Remove(node);
        }
    }

    private static void Loop(object tokenObj)
    {
        var token = (CancellationToken)tokenObj;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var time = stopwatch.Elapsed.TotalSeconds;

        while (!token.IsCancellationRequested)
        {
            _resetEvent.WaitOne();

            lock (NodeAddRemoveEvents)
            {
                while (NodeAddRemoveEvents.TryDequeue(out var evt))
                {
                    // todo - add to undo/redo
                    if (evt.Add)
                        Nodes.Add(evt.Node);
                    else
                        Nodes.Remove(evt.Node);
                }

                var pTime = time;
                time = stopwatch.Elapsed.TotalSeconds;
                var deltaTime = time - pTime;
                foreach (var node in Nodes)
                {
                    node.Process(deltaTime);
                }
            }
        }
    }

    public static void Start()
    {
        _cts = new CancellationTokenSource();
        var thread = new Thread(Loop!);
        thread.Start(_cts.Token);
        _mainLoop = thread;
    }

    public static void Stop()
    {
        if (_mainLoop == null)
            throw new InvalidOperationException("ProcessLoop is not running");
        
        if (_cts == null)
            throw new InvalidOperationException("Cancellation token source was not initialized");

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
        _resetEvent.WaitOne();
        _mainLoop.Join();
        _mainLoop = null;
    }

    public static void AllowRunOnce() => _resetEvent.Set();

    private static bool IsRunning => _mainLoop != null;
    private static Thread? _mainLoop;

    private static CancellationTokenSource? _cts;
    private static readonly AutoResetEvent _resetEvent = new(false);
    private static readonly Queue<NodeAddRemoveEvent> NodeAddRemoveEvents = new();
    private static readonly List<GraphNodeLogic> Nodes = new();

    private readonly record struct NodeAddRemoveEvent(GraphNodeLogic Node, bool Add);
}