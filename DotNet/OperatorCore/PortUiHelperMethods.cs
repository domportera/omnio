using NodeGraphEditor.Editor;
using NodeGraphEditor.Engine;
using NodeGraphEditor.GraphNodes;

namespace NodeGraphEditor;

internal static class PortUiHelper
{
    public static T GenerateInputSlotControl<T>(InputSlot<T> inputSlot, Func<InputSlot<T>, T> constructor)
    {
        return constructor(inputSlot);
    }

    public static T GenerateOutputSlotControl<T>(OutputSlot<T> outputSlot, Func<OutputSlot<T>, T> constructor)
    {
        return constructor(outputSlot);
    }
}