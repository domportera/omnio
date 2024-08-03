using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NodeGraphEditor")]

namespace OperatorCore;

internal sealed class RootCanvasNode : GraphNodeLogic
{
    protected override void OnInitialize()
    {
    }

    public override void Process(double delta)
    {
    }

    protected override void OnDestroy()
    {
    }
}

public abstract partial class GraphNodeLogic
{
    internal event Action? Destroyed;

    // this is a unique identifier for the instance of the node
    private Guid _instanceId = Guid.Empty;

    internal Guid InstanceId
    {
        get => _instanceId;
        set
        {
            if (_instanceId != Guid.Empty)
            {
                throw new InvalidOperationException("Instance ID can only be set once");
            }

            _instanceId = value;
            InstanceIdString = value.ToString();

            Init();
        }
    }

    internal string InstanceIdString { get; private set; }

    protected internal GraphNodeLogic()
    {
    }
    
    public void LoadInstanceInfo(InstanceInfo data)
    {
        // data includes:
        // specific input values for this instance (if not connected)
    }

    internal void SetReady()
    {
        if (InstanceIdString == null)
            throw new InvalidOperationException("Instance ID must be set before calling SetReady");

        OnInitialize();
    }

    protected abstract void OnInitialize();
    public abstract void Process(double delta);

    protected abstract void OnDestroy();

    internal void Destroy()
    {
        if (_isDestroyed)
        {
            return;
        }

        _isDestroyed = true;
        Destroyed?.Invoke();
        
        foreach(var input in _inputSlots)
            input.DisconnectAll();
        
        foreach(var output in _outputSlots)
            output.DisconnectAll();
        
        OnDestroy();
    }


    private readonly List<IOutputSlot> _defaultInputToOutputs = new();
    private readonly List<IInputSlot> _defaultOutputToInputs = new();
    private readonly List<IInputSlot> _inputSlots = new();
    private readonly List<IOutputSlot> _outputSlots = new();
    private bool _isDestroyed;

    private SubGraph? _subGraph;

    internal SubGraph SubGraph
    {
        get => _subGraph ??= new SubGraph();
        set
        {
            if(_subGraph != null)
                throw new InvalidOperationException("SubGraph can only be set once");
            
            _subGraph = value;
        }
    }

    internal IReadOnlyList<IInputSlot> InputSlots => _inputSlots;
    internal IReadOnlyList<IOutputSlot> OutputSlots => _outputSlots;

    internal bool TryAddConnection(RuntimePortInfo fromPortInfo, RuntimePortInfo toPortInfo)
    {
        var fromNode = SubGraph.GetNode(fromPortInfo.NodeInstanceId);
        var toNode = SubGraph.GetNode(toPortInfo.NodeInstanceId);
        var fromPort = fromNode.GetOutputPort(fromPortInfo.PortIndex);
        var toPort = toNode.GetInputPort(toPortInfo.PortIndex);
        
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
        var fromPort = fromNode.GetOutputPort(fromSlot.PortIndex);
        var toPort = toNode.GetInputPort(toSlot.PortIndex);
        
        toPort.ReleaseConnection(fromPort);
        SubGraph.RemoveConnection(fromNode, fromPort, toNode, toPort);
        return true;
    }

    private IInputSlot GetInputPort(int fromPort)
    {
        return GetAtIndex(fromPort, _inputSlots!)!;
    }
    
    private IOutputSlot GetOutputPort(int fromPort)
    {
        return GetAtIndex(fromPort, _outputSlots!)!;
    }
    private static T GetAtIndex<T>(int fromPort, List<T> collection)
    {
        if(fromPort < 0 || fromPort >= collection!.Count)
            throw new System.ArgumentOutOfRangeException(nameof(fromPort));
        
        var port = collection[fromPort]!;

        if(port == null)
            throw new System.InvalidOperationException($"Port {fromPort} is null");
        
        return port;
    }
}

internal readonly record struct RuntimePortInfo(Guid NodeInstanceId, int PortIndex);
public readonly record struct TypeInfo(Type Type, int TypeIndex, FieldInfo[] Fields);