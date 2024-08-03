using System.ComponentModel;
using System.Runtime.InteropServices;
using Godot;
using OperatorCore;

namespace NodeImplementations;

[Guid("9557ADE7-BEF8-4666-8ACB-C2C9276C69CC")]
[Description("Just a test node"), Category("Useless")]
public class TestNode : GraphNodeLogic
{
    private readonly InputSlot<string> _text = new(0, "this is default");
    private readonly InputSlot<int> _num = new(1, 42);
    
    private readonly OutputSlot<string> _textOutput = new(0);
    private readonly OutputSlot<int> _numberX2 = new(1);
    private readonly OutputSlot<bool> _numberIsEven = new(2);

    protected override void OnInitialize()
    {
        _text.ValueChanged += () => _textOutput.Value = _text.Value;
        _num.ValueChanged += () =>
        {
            _numberX2.Value = _num.Value * 2;
            _numberIsEven.Value = _num.Value % 2 == 0;
        };
    }

    public override void Process(double deltaTime)
    {
    }

    protected override void OnDestroy()
    {
        GD.Print("HELP!!!!!!!!!!! IM DYING!!!!!!!!!!!!!");
    }
}