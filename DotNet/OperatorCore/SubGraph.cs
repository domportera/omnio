using System.Diagnostics.CodeAnalysis;
using Utilities.Logging;

namespace OperatorCore;

/// <summary>
/// data includes:
/// additional exposed inputs and outputs, along with their default values
/// all inner connections
/// every inner graphNode and their instance info
/// </summary>
public partial class SubGraph
{
    // index should match that of _nodeInstances
    [NonSerialized] private readonly Dictionary<Guid, GraphNodeLogic> _instantiatedNodes = new();

    private readonly SubgraphDefinition _definition;

    internal SubGraph(SubgraphDefinition definition)
    {
        _definition = definition;
        foreach (var instanceInfo in definition.NodeInstances)
        {
            if (!TryCreateNodeLogic(instanceInfo, out var nodeLogic))
            {
                LogLady.Error($"Failed to create node logic for instance ID {instanceInfo.InstanceId}");
                continue;
            }
            
            _instantiatedNodes.Add(instanceInfo.InstanceId, nodeLogic);
        }
    }

    public bool TryCreateNewNodeLogic(Guid typeId, [NotNullWhen(true)] out GraphNodeLogic? nodeLogic)
    {
        var instanceId = Guid.NewGuid();
        var instanceInfo = new InstanceInfo(typeId, instanceId);
        _definition.AddNodeInstance(instanceInfo);

        if (TryCreateNodeLogic(instanceInfo, out nodeLogic)) 
            return true;
        
        LogLady.Error($"Failed to create node logic for instance ID {instanceId}");
        return false;
    }

    private bool TryCreateNodeLogic(InstanceInfo instanceInfo, [NotNullWhen(true)] out GraphNodeLogic? nodeLogic)
    {
        var instanceId = instanceInfo.InstanceId;
        
        var typeId = instanceInfo.TypeId;
        
        try
        {
            nodeLogic = GraphNodeLogic.CreateNodeLogic(typeId);
        }
        catch (Exception e)
        {
            LogLady.Error(e);
            nodeLogic = null;
            return false;
        }

        if (!_instantiatedNodes.TryAdd(instanceInfo.InstanceId, nodeLogic))
        {
            throw new InvalidOperationException("Node with this instance ID already exists");
        }

        var subGraph = CreateSubgraphFor(typeId);
        nodeLogic.ApplyRuntimeInfo(subGraph, instanceInfo);

        _instantiatedNodes.Add(instanceId, nodeLogic);
        return true;
    }

    private static SubGraph CreateSubgraphFor(Guid typeId)
    {
        var subgraphDef = GetSubgraphDefinition(typeId);
        var subgraph = new SubGraph(subgraphDef);
        return subgraph;
    }

    public bool RemoveNode(Guid instanceId)
    {
        _definition.RemoveNodeInstance(instanceId);

        if (!_instantiatedNodes.Remove(instanceId, out var logic))
            throw new InvalidOperationException("Node instance info was found but not the logic instance");

        logic.Destroy();
        return true;
    }

    internal void AddConnection(GraphNodeLogic fromLogic, IOutputSlot fromSlot, GraphNodeLogic toLogic,
        IInputSlot toSlot)
    {
        var fromEndpoint = new ConnectionEndpoint(fromLogic.InstanceId, fromSlot.Id);
        var toEndpoint = new ConnectionEndpoint(toLogic.InstanceId, toSlot.Id);
        _definition.AddConnection(new Connection(fromEndpoint, toEndpoint));
    }

    internal void RemoveConnection(GraphNodeLogic fromNode, IOutputSlot fromPort, GraphNodeLogic toNode,
        IInputSlot toPort)
    {
        var fromEndpoint = new ConnectionEndpoint(fromNode.InstanceId, fromPort.Id);
        var toEndpoint = new ConnectionEndpoint(toNode.InstanceId, toPort.Id);
        _definition.RemoveConnection(new Connection(fromEndpoint, toEndpoint));
    }

    internal GraphNodeLogic GetNode(Guid fromSlotNodeInstanceId)
    {
        return _instantiatedNodes[fromSlotNodeInstanceId];
    }
}

[Serializable]
public readonly record struct Connection(in ConnectionEndpoint SourceOutput, in ConnectionEndpoint TargetInput);

[Serializable]
public readonly record struct ConnectionEndpoint(Guid NodeId, ushort PortId);

public readonly record struct AdditionalDefaultInput(ConnectionEndpoint Endpoint, object Value);