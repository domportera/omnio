using System.ComponentModel;
using NodeGraphEditor.Editor;

namespace NodeGraphEditor.Engine;

[ReadOnly(true)]
public sealed class InputSlot<T>(ushort id, T value) : IInputSlot<T>, IInputSlot, IReadOnlySlot<T>
{
    internal readonly ushort Id = id;

    private T _value = value;

    public T Value
    {
        get => _value;
        internal set
        {
            _value = value;
            try
            {
                ValueChanged?.Invoke();
            }
            catch (Exception e)
            {
                // todo - more robust logging with owning node and slot name
                Console.WriteLine(e);
            }
        }
    }

    public string Name { get; private set; } = string.Empty;

    string ISlot.Name
    {
        get => Name;
        set => Name = value;
    }

    T IInputSlot<T>.Value
    {
        set => Value = value;
    }

    Type ISlot.Type => typeof(T);

    public readonly T? DefaultValue = value;
    public event Action? ValueChanged;

    bool IInputSlot.TryConnectTo(IOutputSlot outputSlot)
    {
        if (!outputSlot.TryConnectTo(this)) 
            return false;
        
        if (_connectedOutputSlot != null)
            ReleaseConnection();

        _connectedOutputSlot = outputSlot;
        ConnectionStateChanged?.Invoke(true);
        return true;

    }

    void ISlot.DisconnectAll() => ((IInputSlot)this).ReleaseConnection();

    void IInputSlot.ReleaseConnection() => ReleaseConnection();

    private void ReleaseConnection()
    {
        if (_connectedOutputSlot == null)
            return;

        _connectedOutputSlot.RemoveConnection(this);
        _connectedOutputSlot = null;
        ConnectionStateChanged?.Invoke(false);
    }

    internal event Action<bool>? ConnectionStateChanged;

    event Action<bool> IInputSlot.ConnectionStateChanged
    {
        add => ConnectionStateChanged += value;
        remove => ConnectionStateChanged -= value;
    }

    IInputSlot IInputSlot<T>.Reference => this;
    public bool IsConnected => _connectedOutputSlot != null;

    private IOutputSlot? _connectedOutputSlot;
}