using Godot;
using Label = Godot.Label;

namespace NodeGraphEditor.Editor;

public abstract class PortControl
{
    public Control Control => _control ??= InitializeControl();
    protected virtual bool ShowLabel => true;

    private Control? _control;

    private Control InitializeControl()
    {
        // format name, for example "_mySlotName" -> "MySlotName"
        // todo - format name better
        var slotName = _slot.Name.TrimStart('_');
        slotName = char.ToUpper(slotName[0]) + slotName[1..];

        var isInput = _slot is IInputSlot;
        var parent = new VBoxContainer();
        parent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        parent.SizeFlagsStretchRatio = 1;
        
        parent.Name = string.Format(NameFmt, slotName, nameof(PortControl));

        if (ShowLabel)
        {
            var label = new Label
            {
                Text = slotName,
                Name = string.Format(NameFmt, slotName, nameof(Label)),
                HorizontalAlignment = isInput 
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Right,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            
            parent.AddChild(label);
        }

        var control = CreateControl();
        control.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        control.Name = slotName;
        parent.AddChild(control);

        return parent;
    }

    public void Dispose()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(PortControl));

        _isDisposed = true;
        OnDispose();
        _control?.Dispose();
    }

    protected abstract void OnDispose();

    protected abstract Control CreateControl();

    private ISlot _slot;
    private bool _isDisposed;

    private const string NameFmt = "{0} ({1})";

    public void SetSlot(ISlot slot)
    {
        if(_slot != null)
            throw new InvalidOperationException("Slot already set");
        
        _slot = slot;
    }

    protected T GetSlot<T>() where T : ISlot
    {
        if (_slot is T slot)
            return slot;

        throw new InvalidOperationException($"Slot is not of type {typeof(T)}");
    }
}

internal static class SlotControlExtensionMethods
{
    
}