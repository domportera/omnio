namespace OperatorCore;

internal interface IInputSlot<in T>
{
    T Value { set; }
    public IInputSlot Reference { get; }
}

internal interface IInputSlot : ISlot
{
    public bool TryConnectTo(IOutputSlot outputSlot, bool isTransformation = false);
    event Action<bool> ConnectionStateChanged;

    void ISlot.ActAsTransformationSlot(ISlot slot) => ActAsTransformationSlot((IOutputSlot)slot);
    void ActAsTransformationSlot(IOutputSlot slot)
    {
        #if DEBUG
        if(Type != slot.Type)
            throw new InvalidOperationException("Cannot mimic a slot with a different type");
        #endif
        
        Name = slot.Name;
        Id = slot.Id;
        
        if(!TryConnectTo(slot, true))
            throw new InvalidOperationException("Failed to connect mimicked slot");
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
    void ReleaseConnection(IOutputSlot fromPort);
}

internal interface IReadOnlySlot<out T> : ISlot
{
    T Value { get; }
}

public interface ISlot
{
    Type Type { get; }
    ushort Id { get; internal set; }
    string Name { get; set; }
    void DisconnectAll();
    event Action ValueChanged;

    void ActAsTransformationSlot(ISlot slot);
}

internal interface IOutputSlot : ISlot
{
    public bool TryConnectTo<TInput>(InputSlot<TInput> inputSlot);
    public bool RemoveConnection<TInput>(InputSlot<TInput> inputSlot);

    void ISlot.ActAsTransformationSlot(ISlot slot) => ActAsTransformationSlot((IInputSlot)slot);
    void ActAsTransformationSlot(IInputSlot slot)
    {
        #if DEBUG
        if(Type != slot.Type)
            throw new InvalidOperationException("Cannot mimic a slot with a different type");
        #endif
        
        Name = slot.Name;
        Id = slot.Id;
        
        if(!slot.TryConnectTo(this, true))
            throw new InvalidOperationException("Failed to connect mimicked slot");
    }
}