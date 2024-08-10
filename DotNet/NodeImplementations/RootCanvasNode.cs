using System.Runtime.InteropServices;
using OperatorCore;

namespace NodeImplementations;

[Guid("00000000-0000-0000-0000-000000000000"), Hidden]
internal sealed class RootCanvasNode : GraphNodeLogic
{
    protected override void OnInitialize()
    {
    }

    public override void Process(double deltaTime)
    {
    }

    protected override void OnDestroy()
    {
    }
}
