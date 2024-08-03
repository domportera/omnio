using System.Diagnostics.CodeAnalysis;
using Utilities.Logging;

namespace OperatorCore;

/// <summary>
/// data includes:
/// additional exposed inputs and outputs, along with their default values
/// all inner connections
/// every inner graphNode and their instance info
/// </summary>
[Serializable]
public class SubGraph
{
    // index should match that of _nodeInstances
    [NonSerialized] private readonly Dictionary<Guid, GraphNodeLogic> _nodes = new();

    // key is instance Id, value is type Id
    private readonly Dictionary<Guid, InstanceInfo> _nodeInstances = new();
    private readonly List<AdditionalDefaultInput> _additionalExposedInputs = [];
    private readonly List<ConnectionEndpoint> _additionalExposedOutputs = [];

    private readonly List<Connection> _connections = [];

    public bool TryCreateNodeLogic(Guid typeId, [NotNullWhen(true)] out GraphNodeLogic? nodeLogic,
        Guid instanceId = default)
    {
        var type = TypeCache.GetTypeById(typeId);
        try
        {
            nodeLogic = GraphNodeLogic.CreateNodeLogic(type);
        }
        catch (Exception e)
        {
            LogLady.Error(e);
            nodeLogic = null;
            return false;
        }

        if (instanceId == default)
            instanceId = Guid.NewGuid();

        nodeLogic.InstanceId = instanceId;

        _nodeInstances.Add(instanceId, new InstanceInfo(typeId, instanceId));
        _nodes.Add(instanceId, nodeLogic);
        return true;
    }

    public bool RemoveNode(Guid instanceId)
    {
        if (!_nodeInstances.Remove(instanceId))
            return false;

        if (!_nodes.Remove(instanceId, out var logic))
            throw new InvalidOperationException("Node instance info was found but not the logic instance");

        logic.Destroy();
        return true;
    }

    internal void AddConnection(GraphNodeLogic fromLogic, IOutputSlot fromSlot, GraphNodeLogic toLogic,
        IInputSlot toSlot)
    {
        var fromEndpoint = new ConnectionEndpoint(fromLogic.InstanceId, fromSlot.Id);
        var toEndpoint = new ConnectionEndpoint(toLogic.InstanceId, toSlot.Id);
        _connections.Add(new Connection(fromEndpoint, toEndpoint));
    }

    internal void RemoveConnection(GraphNodeLogic fromNode, IOutputSlot fromPort, GraphNodeLogic toNode, IInputSlot toPort)
    {
        var fromEndpoint = new ConnectionEndpoint(fromNode.InstanceId, fromPort.Id);
        var toEndpoint = new ConnectionEndpoint(toNode.InstanceId, toPort.Id);
        _connections.Remove(new Connection(fromEndpoint, toEndpoint));
    }

    internal GraphNodeLogic GetNode(Guid fromSlotNodeInstanceId)
    {
        return _nodes[fromSlotNodeInstanceId];
    }
}

[Serializable]
public readonly record struct Connection(in ConnectionEndpoint SourceOutput, in ConnectionEndpoint TargetInput);

[Serializable]
public readonly record struct ConnectionEndpoint(Guid NodeId, ushort PortId);

public readonly record struct AdditionalDefaultInput(ConnectionEndpoint Endpoint, object Value);