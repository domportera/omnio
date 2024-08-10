using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NodeGraphEditor")]

namespace OperatorCore;

internal sealed class RootCanvasNode : GraphNodeLogic
{
    protected override void OnInitialize()
    {
    }

    public override void Process(double deltaTime)
    {
    }

    protected override void OnDestroy()
    {
    }
}

public abstract partial class GraphNodeLogic
{
    protected internal GraphNodeLogic()
    {
    }
    
    internal event Action? Destroyed;

    public Guid InstanceId
    {
        get => _instanceId;
        private set
        {
            if (_instanceId != default)
            {
                throw new InvalidOperationException("Instance ID can only be set once");
            }

            _instanceId = value;
            InstanceIdString = value.ToString();
        }
    }

    internal SubGraph SubGraph
    {
        get => _subGraph!;
        private set
        {
            if (_subGraph != null)
                throw new InvalidOperationException("Subgraph can only be set once");

            _subGraph = value;
        }
    }

    internal string InstanceIdString { get; private set; } = null!;
    internal IReadOnlyList<IInputSlot> InputSlots => _inputSlots;
    internal IReadOnlyList<IOutputSlot> OutputSlots => _outputSlots;

    protected abstract void OnInitialize();
    public abstract void Process(double deltaTime);

    protected abstract void OnDestroy();

    internal void Destroy()
    {
        if (_isDestroyed)
        {
            return;
        }

        ProcessLoop.Remove(this);

        _isDestroyed = true;
        Destroyed?.Invoke();

        foreach (var input in _inputSlots)
            input.DisconnectAll();

        foreach (var output in _outputSlots)
            output.DisconnectAll();

        OnDestroy();
    }

    internal bool TryAddConnection(RuntimePortInfo fromPortInfo, RuntimePortInfo toPortInfo)
    {
        var fromNode = SubGraph.GetNode(fromPortInfo.NodeInstanceId);
        var toNode = SubGraph.GetNode(toPortInfo.NodeInstanceId);
        var fromPort = fromNode.GetOutputPortInternal(fromPortInfo.PortIndex);
        var toPort = toNode.GetInputPortInternal(toPortInfo.PortIndex);

        if (!toPort.TryConnectTo(fromPort))
        {
            return false;
        }

        SubGraph.AddConnection(fromNode, fromPort, toNode, toPort);
        return true;
    }

    internal bool RemoveConnection(RuntimePortInfo fromSlot, RuntimePortInfo toSlot)
    {
        var fromNode = SubGraph.GetNode(fromSlot.NodeInstanceId);
        var toNode = SubGraph.GetNode(toSlot.NodeInstanceId);
        var fromPort = fromNode.GetOutputPortInternal(fromSlot.PortIndex);
        var toPort = toNode.GetInputPortInternal(toSlot.PortIndex);

        toPort.ReleaseConnection(fromPort);
        SubGraph.RemoveConnection(fromNode, fromPort, toNode, toPort);
        return true;
    }

    private IInputSlot GetInputPortInternal(int fromPort)
    {
        return GetAtIndex(fromPort, _inputSlots!)!;
    }

    private IOutputSlot GetOutputPortInternal(int fromPort)
    {
        return GetAtIndex(fromPort, _outputSlots!)!;
    }
    
    public ISlot GetInputPort(int fromPort)
    {
        return GetAtIndex(fromPort, _inputSlots!);
    }
    
    public ISlot GetOutputPort(int fromPort)
    {
        return GetAtIndex(fromPort, _outputSlots!);
    }
    

    private static T GetAtIndex<T>(int fromPort, List<T> collection)
    {
        if (fromPort < 0 || fromPort >= collection!.Count)
            throw new System.ArgumentOutOfRangeException(nameof(fromPort));

        var port = collection[fromPort]!;

        if (port == null)
            throw new System.InvalidOperationException($"Port {fromPort} is null");

        return port;
    }


    private readonly List<IOutputSlot> _defaultInputToOutputs = [];
    private readonly List<IInputSlot> _defaultOutputToInputs = [];
    private readonly List<IInputSlot> _inputSlots = [];
    private readonly List<IOutputSlot> _outputSlots = [];
    private bool _isDestroyed;

    private SubGraph? _subGraph;

    // this is a unique identifier for the instance of the node
    private Guid _instanceId;
}

internal readonly record struct RuntimePortInfo(Guid NodeInstanceId, int PortIndex);
