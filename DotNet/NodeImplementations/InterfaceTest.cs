using System.Runtime.InteropServices;
using OperatorCore;

namespace NodeImplementations;

[Guid("7197ED02-B2BF-4403-B30D-19D327EE6BC6")]
public class InterfaceTest : GraphNodeLogic
{
    protected override void OnInitialize()
    {
        string cat = "";
    }

    public override void Process(double deltaTime)
    {
    }

    protected override void OnDestroy()
    {
    }
}