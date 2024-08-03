using System.Collections.Generic;
using Godot;
using NodeGraphEditor.GraphNodeUI;

namespace NodeGraphEditor;

internal static class UiRunner
{
    internal static void AddPortControl(PortControl portControl)
    {
        _portControls.Add(portControl);
    }
    
    internal static void RemovePortControl(PortControl portControl)
    {
        _portControls.Remove(portControl);
    }
    
    public static void Do()
    {
        foreach(var portControl in _portControls)
            portControl.UpdateValueIfDirty();
    }
    
    private static readonly List<PortControl> _portControls = new();
}