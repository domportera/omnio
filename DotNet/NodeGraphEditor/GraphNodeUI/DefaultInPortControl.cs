using System;
using Godot;
using NodeGraphEditor.UI;
using OperatorCore;

namespace NodeGraphEditor.GraphNodeUI;

internal sealed class DefaultInPortControl<T> : PortControl
{
    protected override Control CreateControl()
    {
        if (!FromStringMethods.TryGet<T>(out var stringToValueMethod))
        {
            var labelDisplay = DefaultTextDisplay.CreateLineEdit("", HorizontalAlignment.Left);
            _textDisplay = labelDisplay;
            labelDisplay.Control.Editable = false;
            GD.PrintErr($"No conversion method found for {typeof(string)} to {typeof(T)}");
            return labelDisplay.Control;
        }

        var lineEditDisplay = DefaultTextDisplay.CreateLineEdit("", HorizontalAlignment.Left);
        _textDisplay = lineEditDisplay;


        lineEditDisplay.TextChanged += text =>
        {
            var slot = GetSlot<InputSlot<T>>();
            if (slot.IsConnected)
                return;

            if (stringToValueMethod.Invoke(text, out var value))
                slot.Value = value;
        };


        return lineEditDisplay.Control;
    }

    protected override void OnValueChanged()
    {
        var slot = GetSlot<InputSlot<T>>();
        if (slot.IsConnected)
        {
            _textDisplay!.SetTextSilently(_valueToString(slot.Value));
        }
    }

    protected override void OnConnectionStateChanged(bool isConnected)
    {
        if (isConnected)
            MarkValueDirty();
    }

    protected override void OnDispose() => _textDisplay?.Dispose();

    private readonly ToStringMethod<T> _valueToString = ToStringMethods.Get<T>();
    private ITextDisplay? _textDisplay;
}