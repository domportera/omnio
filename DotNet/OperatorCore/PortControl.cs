using Godot;
using Label = Godot.Label;

namespace NodeGraphEditor.Editor;

public abstract class PortControl
{
    public Control Control => _control ??= InitializeControl();
    protected virtual bool ShowLabel => true;

    private Control? _control;

    protected PortControl(ISlot slot)
    {
        _slot = slot;
    }

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

    private readonly ISlot _slot;
    private bool _isDisposed;

    private const string NameFmt = "{0} ({1})";
}

internal static class SlotControlExtensionMethods
{
    
}