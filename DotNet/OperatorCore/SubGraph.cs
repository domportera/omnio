using System.Diagnostics.CodeAnalysis;

namespace OperatorCore;

[Serializable]
public class SubGraph
{
    // index should match that of _nodeInstances
    [NonSerialized]
    readonly Dictionary<Guid, GraphNodeLogic> _nodes = new();
    
    // key is instance Id, value is type Id
    readonly Dictionary<Guid, Guid> _nodeInstances = new();
    
    readonly List<Connection> _connections = new();
    readonly List<ConnectionEndpoint> _exposedInputs = new();
    readonly List<ConnectionEndpoint> _exposedOutputs = new();

    public bool TryCreateNodeLogic(Guid typeId, [NotNullWhen(true)] out GraphNodeLogic? nodeLogic, Guid instanceId = default)
    {
        var type = TypeCache.GetTypeById(typeId);
        try
        {
            nodeLogic = GraphNodeLogic.CreateNodeLogic(type);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            nodeLogic = null;
            return false;
        }
        
        if(instanceId == default)
            instanceId = Guid.NewGuid();
        
        nodeLogic.InstanceId = instanceId;
        
        _nodeInstances.Add(instanceId, typeId);
        _nodes.Add(instanceId, nodeLogic);
        return true;
    }

    public bool TryRemoveNode(Guid instanceId)
    {
        if(!_nodeInstances.Remove(instanceId))
            return false;
        
        if(!_nodes.Remove(instanceId, out var logic))
            throw new InvalidOperationException("Node instance ID was found but not the logic instance");
        
        logic.Destroy();
        return true;
    }

    internal void AddConnection(GraphNodeLogic fromLogic, IOutputSlot fromSlot, GraphNodeLogic toLogic, IInputSlot toSlot)
    {
        var fromEndpoint = new ConnectionEndpoint(fromLogic.InstanceId, fromSlot.Id);
        var toEndpoint = new ConnectionEndpoint(toLogic.InstanceId, toSlot.Id);
        _connections.Add(new Connection(fromEndpoint, toEndpoint));
    }

    internal GraphNodeLogic GetNode(Guid fromSlotNodeInstanceId)
    {
        return _nodes[fromSlotNodeInstanceId];
    }
}

[System.Serializable]
public readonly record struct Connection(in ConnectionEndpoint SourceOutput, in ConnectionEndpoint TargetInput);

[System.Serializable]
public readonly record struct ConnectionEndpoint(Guid NodeId, ushort PortId);