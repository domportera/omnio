using System.Runtime.InteropServices;
using System.Text;
using NodeGraphEditor.Engine;
using NodeGraphEditor.GraphNodes;
using OperatorCore;

namespace NodeGraphEditor.NodeImplementations;

[Guid("61F89227-6810-49AB-9848-069DADB63B16")]
public class CountSeconds : GraphNodeLogic
{
    private readonly InputSlot<int> _countInterval = new(1, 42);
    
    private readonly OutputSlot<string> _countAsString = new(0, "0");
    private readonly OutputSlot<int> _count = new(1, 0);
    protected override void OnInitialize()
    {
        
    }

    public override void Process(double delta)
    {
        _timer += delta;
        _count.Value = (int)(_timer * _countInterval.Value);
        
        _stringBuilder.Append(_count.Value);
        _stringBuilder.Append('s');
        _countAsString.Value = _stringBuilder.ToString();
        _stringBuilder.Clear();
    }

    protected override void OnDestroy()
    {
    }

    private double _timer = 0;
    private readonly StringBuilder _stringBuilder = new();
}