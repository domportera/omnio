using System.Collections.Concurrent;

namespace OperatorCore;

[Serializable]
internal class SubgraphDefinition
{

    internal SubgraphDefinition(Guid typeId)
    {
        _typeId = typeId;
    }
    
    private readonly Guid _typeId;
    
    // key is instance Id, value is type Id
    internal IReadOnlyCollection<InstanceInfo> NodeInstances => _nodeInstances;
    private readonly List<InstanceInfo> _nodeInstances = new();
    private readonly List<AdditionalDefaultInput> _additionalExposedInputs = [];
    private readonly List<ConnectionEndpoint> _additionalExposedOutputs = [];
    private readonly List<Connection> _connections = [];
    
    public void AddNodeInstance(InstanceInfo instanceInfo)
    {
        if(_nodeInstances.Any(x => x.InstanceId == instanceInfo.InstanceId))
            throw new InvalidOperationException("Node with this instance ID already exists");
        
        _nodeInstances.Add(instanceInfo);
        InstanceAdded?.Invoke(instanceInfo);
    }
    
    public void RemoveNodeInstance(Guid instanceId)
    {
        var index = _nodeInstances.FindIndex(x => x.InstanceId == instanceId);
        
        if(index == -1)
            throw new InvalidOperationException("Node with this instance ID does not exist");
        
        var removedInstance = _nodeInstances[index];
        _nodeInstances.RemoveAt(index);
        InstanceRemoved?.Invoke(removedInstance);
    }
    
    public void AddConnection(Connection connection)
    {
        _connections.Add(connection);
        ConnectionAdded?.Invoke(connection);
    }
    
    public void RemoveConnection(Connection connection)
    {
        if(!_connections.Remove(connection))
            throw new InvalidOperationException("Connection does not exist");
        
        ConnectionRemoved?.Invoke(connection);
    }
    
    public void AddOutput(ConnectionEndpoint output)
    {
        if(_additionalExposedOutputs.Contains(output))
            throw new InvalidOperationException("Output already exists");
        
        _additionalExposedOutputs.Add(output);
        OutputAdded?.Invoke(output);
    }
    
    public void RemoveOutput(ConnectionEndpoint output)
    {
        if(!_additionalExposedOutputs.Remove(output))
            throw new InvalidOperationException("Output does not exist");
        
        OutputRemoved?.Invoke(output);
    }
    
    public void AddInput(AdditionalDefaultInput input)
    {
        if(_additionalExposedInputs.Contains(input))
            throw new InvalidOperationException("Input already exists");
        
        _additionalExposedInputs.Add(input);
        InputAdded?.Invoke(input);
    }
    
    public void RemoveInput(AdditionalDefaultInput input)
    {
        if(!_additionalExposedInputs.Remove(input))
            throw new InvalidOperationException("Input does not exist");
        
        InputRemoved?.Invoke(input);
    }

    internal event Action<InstanceInfo>? InstanceAdded;
    internal event Action<InstanceInfo>? InstanceRemoved;

    internal event Action<Connection>? ConnectionAdded;
    internal event Action<Connection>? ConnectionRemoved;
    internal event Action<ConnectionEndpoint>? OutputAdded;
    internal event Action<ConnectionEndpoint>? OutputRemoved;
    
    internal event Action<AdditionalDefaultInput>? InputAdded;
    internal event Action<AdditionalDefaultInput>? InputRemoved;
}