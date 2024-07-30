using System.ComponentModel;

// ReSharper disable ForCanBeConvertedToForeach

namespace OperatorCore;

[ReadOnly(true)]
public sealed class OutputSlot<T> : IOutputSlot, IReadOnlySlot<T>
{
    public OutputSlot(ushort id, T value)
    {
        Id = id;
        _value = value;
    }
    
    internal ushort Id { get; private set; }
    ushort ISlot.Id { get => Id; set => Id = value; }
    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            for (int i = 0; i < _connectedInputSlots.Count; i++)
                _connectedInputSlots[i].Value = value;
            
            ValueChanged?.Invoke();
        }
    }
    
    public string Name { get; private set; } = string.Empty;
    string ISlot.Name
    {
        get => this.Name;
        set => this.Name = value;
    }

    Type ISlot.Type => typeof(T);
    event Action? ISlot.ValueChanged
    {
        add => ValueChanged += value;
        remove => ValueChanged -= value;
    }
    
    internal event Action? ValueChanged;

    bool IOutputSlot.RemoveConnection<TInput>(InputSlot<TInput> inputSlot)
    {
        var slotCount = _connectedInputSlots.Count;
        for (var index = 0; index < slotCount; index++)
        {
            var slot = _connectedInputSlots[index];
            if (slot.Reference != inputSlot)
                continue;

            _connectedInputSlots.Remove(slot);
            return true;
        }

        return false;
    }

    bool IOutputSlot.TryConnectTo<TInput>(InputSlot<TInput> inputSlot)
    {
        ArgumentNullException.ThrowIfNull(inputSlot);

        ((IOutputSlot)this).RemoveConnection(inputSlot);

        if (inputSlot is InputSlot<T> compatible)
        {
            _connectedInputSlots.Add(compatible);
            compatible.Value = _value;
            return true;
        }

        if (!typeof(T).IsAssignableTo(typeof(TInput)))
            return false;

        var wrapperType = typeof(InputSlotWrapper<,>).MakeGenericType(typeof(TInput), typeof(T));
        try
        {
            // optimize - compile this as a constructor expression?
            var wrapper = Activator.CreateInstance(wrapperType, inputSlot);
            if (wrapper == null)
                return false;

            var connectedInputSlot = (IInputSlot<T>)wrapper;
            _connectedInputSlots.Add(connectedInputSlot);
            connectedInputSlot.Value = _value;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private readonly List<IInputSlot<T>> _connectedInputSlots = [];
    
    // for use with reflection only - needs a default constructor
    // ReSharper disable once UnusedMember.Local
    private OutputSlot()
    {
    }

    void ISlot.DisconnectAll()
    {
        while (_connectedInputSlots.Count > 0)
        {
            // this will end up calling RemoveConnection for each, which will remove the slot from the list
            _connectedInputSlots[^1].Reference.ReleaseConnection(this);
        }
    }

}

file sealed class InputSlotWrapper<TInput, TOutput>(InputSlot<TInput> inputSlot) : IInputSlot<TOutput>
    where TOutput : TInput
{
    public TOutput Value
    {
        set => inputSlot.Value = value;
    }

    public IInputSlot Reference => inputSlot;
}