using System;
using Godot;
using NodeGraphEditor.UI;
using OperatorCore;

namespace NodeGraphEditor.GraphNodeUI;

internal sealed class DefaultOutPortControl<T> : PortControl
{
    protected override Control CreateControl()
    {
        var toStringMethod = ToStringMethods.Get<T>();
        var slot = GetSlot<OutputSlot<T>>();
        var str = toStringMethod(slot.Value);
        var textDisplay = DefaultTextDisplay.CreateLineEdit(str, HorizontalAlignment.Right);
        textDisplay.Control.Editable = false;
        _displayLabel = textDisplay;
        return textDisplay.Control;
    }

    protected override void OnDispose() => _displayLabel?.Dispose();

    protected override void OnValueChanged()
    {
        var slot = GetSlot<OutputSlot<T>>();
        _displayLabel!.SetTextSilently(ValueToString(slot.Value));
    }
    
    protected override void OnConnectionStateChanged(bool isConnected) { }

    private static readonly ToStringMethod<T> ValueToString = ToStringMethods.Get<T>();
    private ITextDisplay? _displayLabel;
}