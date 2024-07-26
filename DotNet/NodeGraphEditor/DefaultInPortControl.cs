using System;
using Godot;
using NodeGraphEditor.Engine;
using NodeGraphEditor.GraphNodes;

namespace NodeGraphEditor.Editor;

internal sealed class DefaultInPortControl<T> : PortControl
{
    public DefaultInPortControl(InputSlot<T> slot) : base(slot)
    {
        _slot = slot;
    }

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
            if (_slot.IsConnected)
                return;

            if (stringToValueMethod.Invoke(text, out var value))
                _slot.Value = value;
        };


        return lineEditDisplay.Control;
    }
    
    private void LinkSlotToDisplay()
    {
        _valueChanged = () =>
        {
            if (_slot.IsConnected)
            {
                _textDisplay!.SetTextSilently(_valueToString(_slot.Value));
            }
        };
        
        _slot.ValueChanged += _valueChanged;
       // _slot.ConnectionStateChanged += isConnected =>
        //{
          //  _textDisplay!.Control.Editable = !isConnected;
        //};
    }

    protected override void OnDispose()
    {
        _slot.ValueChanged -= _valueChanged;

        _textDisplay?.Dispose();
    }

    private readonly ToStringMethod<T> _valueToString = ToStringMethods.Get<T>();
    private ITextDisplay? _textDisplay;
    private Action? _valueChanged;
    private readonly InputSlot<T> _slot;

}