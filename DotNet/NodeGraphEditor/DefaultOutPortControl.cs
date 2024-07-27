using System;
using Godot;
using NodeGraphEditor.GraphNodes;

namespace NodeGraphEditor.Editor;

// todo - split into two classes
internal sealed class DefaultOutPortControl<T> : PortControl
{
    protected override Control CreateControl()
    {
        var toStringMethod = ToStringMethods.Get<T>();
        var slot = GetSlot<OutputSlot<T>>();
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
        GetSlot<OutputSlot<T>>().ValueChanged -= _valueChanged;
        _displayLabel?.Dispose();
    }

    private Action? _valueChanged;
    private ITextDisplay? _displayLabel;
}