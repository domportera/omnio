using System;
using Godot;
using Utilities.Logging;

namespace NodeGraphEditor;

public partial class AutoLoad : Node
{
    private static AutoLoad? _instance;
    public AutoLoad()
    {
        if(_instance != null)
            throw new InvalidOperationException("AutoLoad already loaded");
        _instance = this;
        NodeImplementations.TypeRegistration.FindAndRegisterTypes();
        LogLady.AddLogger<GDLogger>();
        LogLady.AddLogger<ConsoleLogger>();
    }
    
    ~AutoLoad()
    {
        if (_instance != this)
            return;
        
        _instance = null;
        LogLady.RemoveAllLoggers();
    }
}