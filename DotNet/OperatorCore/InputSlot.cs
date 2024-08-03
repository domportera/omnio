using System.ComponentModel;
using Utilities.Logging;

namespace OperatorCore;

[ReadOnly(true)]
public sealed class InputSlot<T> : IInputSlot<T>, IInputSlot, IReadOnlySlot<T>
{
    public InputSlot(ushort id, T value)
    {
        Id = id;
        _value = value;
        DefaultValue = value;
    }

    internal ushort Id { get; private set; }

    ushort ISlot.Id
    {
        get => Id;
        set => Id = value;
    }

    private T? _value;

    public T? Value
    {
        get => _value;
        internal set
        {
            if (ValueChanged == null)
            {
                _value = value;
                return;
            }

            try
            {
                lock (LockObject)
                {
                    _value = value;
                    ValueChanged.Invoke();
                }
            }
            catch (Exception e)
            {
                LogLady.Error(e);
            }
        }
    }

    public string Name { get; private set; } = string.Empty;

    string ISlot.Name
    {
        get => Name;
        set => Name = value;
    }

    T? IInputSlot<T>.Value
    {
        set => Value = value;
    }

    Type ISlot.Type => typeof(T);

    public readonly T? DefaultValue;
    public event Action? ValueChanged;
    public object LockObject { get; } = new();

    bool IInputSlot.TryConnectTo(IOutputSlot outputSlot, bool isTransformation)
    {
        if (!outputSlot.TryConnectTo(this))
            return false;

        if (!isTransformation)
        {
            if (_connectedOutputSlot != null)
                DisconnectAll();

            _connectedOutputSlot = outputSlot;
            ConnectionStateChanged?.Invoke(true);
        }
        else
        {
            if (_transformationSlot != null)
                throw new InvalidOperationException(
                    "Transformation slot already set - multiple connections not yet supported");

            _transformationSlot = outputSlot;
        }

        return true;
    }

    void ISlot.DisconnectAll() => DisconnectAll();


    private void DisconnectAll()
    {
        if (_connectedOutputSlot == null)
            return;

        _connectedOutputSlot.RemoveConnection(this);
        _connectedOutputSlot = null;
        ConnectionStateChanged?.Invoke(false);
    }

    void IInputSlot.ReleaseConnection(IOutputSlot slot)
    {
        if (slot == _transformationSlot)
        {
            _transformationSlot.RemoveConnection(this);
            _transformationSlot = null;
            return;
        }

        if (_connectedOutputSlot != slot)
            throw new InvalidOperationException("Cannot release connection to a slot that is not connected");

        DisconnectAll();
    }

    internal event Action<bool>? ConnectionStateChanged;

    event Action<bool> ISlot.ConnectionStateChanged
    {
        add => ConnectionStateChanged += value;
        remove => ConnectionStateChanged -= value;
    }

    IInputSlot IInputSlot<T>.Reference => this;
    public bool IsConnected => _connectedOutputSlot != null;

    private IOutputSlot? _connectedOutputSlot;

    private IOutputSlot? _transformationSlot;

    // for use with reflection only - needs a default constructor
    // ReSharper disable once UnusedMember.Local
    private InputSlot()
    {
    }
}