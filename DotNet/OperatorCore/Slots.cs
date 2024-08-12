namespace OperatorCore;

internal interface IInputSlot<in T>
{
    T? Value { set; }
    public IInputSlot Reference { get; }
}

internal interface IInputSlot : ISlot
{
    public bool TryAcceptConnectionFrom(IOutputSlot outputSlot);

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
    void ReleaseConnection(IOutputSlot fromPort);
    void ApplyInputValue(InputValue kvpValue);
    void ForceUpdate();
}

internal interface IReadOnlySlot<out T> : ISlot
{
    T? Value { get; }
}

public interface ISlot
{
    Type Type { get; }
    ushort Id { get; internal set; }
    string Name { get; set; }
    void DisconnectAll();
    event Action ValueChanged;
    event Action<bool> ConnectionStateChanged;

    void ActAsTransformationSlotFor(ISlot slot);
    public object LockObject { get; }
}

internal interface IOutputSlot : ISlot
{
    public bool TryConnectTo<TInput>(InputSlot<TInput> inputSlot, bool isTransformingMe);
    public bool RemoveConnection<TInput>(InputSlot<TInput> inputSlot);
}