using System;
using Godot;
using NodeGraphEditor.GraphNodes;

namespace NodeGraphEditor.Editor;

// todo - split into two classes
internal sealed class DefaultOutPortControl<T>(OutputSlot<T> slot) : PortControl(slot)
{
    protected override Control CreateControl()
    {
        var toStringMethod = ToStringMethods.Get<T>();
        var str = toStringMethod(slot.Value);
        var textDisplay = DefaultTextDisplay.CreateLineEdit(str, HorizontalAlignment.Right);
        textDisplay.Control.Editable = false;
        _valueChanged = () => textDisplay.SetTextSilently(toStringMethod(slot.Value));
        slot.ValueChanged += _valueChanged;
        _displayLabel = textDisplay;
        return textDisplay.Control;
    }

    protected override void OnDispose()
    {
        slot.ValueChanged -= _valueChanged;
        _displayLabel?.Dispose();
    }

    private Action? _valueChanged;
    private ITextDisplay? _displayLabel;
}