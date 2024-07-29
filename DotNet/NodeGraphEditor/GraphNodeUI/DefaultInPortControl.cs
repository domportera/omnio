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
            LinkSlotToDisplay();
            return labelDisplay.Control;
        }

        var lineEditDisplay = DefaultTextDisplay.CreateLineEdit("", HorizontalAlignment.Left);
        _textDisplay = lineEditDisplay;

        LinkSlotToDisplay();

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

    private void LinkSlotToDisplay()
    {
        _valueChanged = () =>
        {
            var slot = GetSlot<InputSlot<T>>();
            if (slot.IsConnected)
            {
                _textDisplay!.SetTextSilently(_valueToString(slot.Value));
            }
        };

        _connectionStateChanged = isConnected =>
        {
            if (isConnected) // unnecessary check, but it's here for clarity and future-proofing
                _valueChanged!.Invoke();
        };

        var slot = GetSlot<InputSlot<T>>();
        slot.ValueChanged += _valueChanged;
        slot.ConnectionStateChanged += _connectionStateChanged;
        _textDisplay!.SetTextSilently(_valueToString(slot.Value));
    }

    protected override void OnDispose()
    {
        var slot = GetSlot<InputSlot<T>>();
        slot.ValueChanged -= _valueChanged;
        slot.ConnectionStateChanged -= _connectionStateChanged;

        _textDisplay?.Dispose();
    }

    private readonly ToStringMethod<T> _valueToString = ToStringMethods.Get<T>();
    private ITextDisplay? _textDisplay;
    private Action? _valueChanged;
    private Action<bool>? _connectionStateChanged;
}