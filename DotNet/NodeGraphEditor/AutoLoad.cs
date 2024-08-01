using System;
using Godot;

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
    }
    public override void _Ready()
    {
    }
    
    ~AutoLoad()
    {
        if(_instance == this)
            _instance = null;
    }
}