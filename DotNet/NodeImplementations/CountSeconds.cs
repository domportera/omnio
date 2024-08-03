using System.Runtime.InteropServices;
using System.Text;
using OperatorCore;

namespace NodeImplementations;

[Guid("61F89227-6810-49AB-9848-069DADB63B16")]
public class CountSeconds : GraphNodeLogic
{
    private readonly InputSlot<double> _countRate = new(1, 1);
    
    private readonly OutputSlot<string> _countAsString = new(0);
    private readonly OutputSlot<int> _count = new(1);
    protected override void OnInitialize()
    {
        
    }

    public override void Process(double deltaTime)
    {
        _timer += deltaTime * _countRate.Value;
        _count.Value = (int)_timer;
        
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