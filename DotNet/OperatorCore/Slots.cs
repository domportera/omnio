using NodeGraphEditor.Engine;
using NodeGraphEditor.GraphNodes;

namespace NodeGraphEditor.Editor;

internal interface IInputSlot<in T>
{
    T Value { set; }
    public IInputSlot Reference { get; }
}

internal interface IInputSlot : ISlot
{
    public bool TryConnectTo(IOutputSlot outputSlot);
    void ReleaseConnection();
    event Action<bool> ConnectionStateChanged;
}

internal interface IReadOnlySlot<out T> : ISlot
{
    T Value { get; }
}

public interface ISlot
{
    Type Type { get; }
    string Name { get; set; }
    void DisconnectAll();
    event Action ValueChanged;
}

internal interface IOutputSlot : ISlot
{
    public bool TryConnectTo<TInput>(InputSlot<TInput> inputSlot);
    public bool RemoveConnection<TInput>(InputSlot<TInput> inputSlot);
}