using System;
using System.Linq;
using System.Reflection;
using Godot;
using OperatorCore;
using Utilities.Logging;

namespace NodeGraphEditor;

public partial class AutoLoad : Node
{
    private static AutoLoad? _instance;

    public AutoLoad()
    {
        if (_instance != null)
            throw new InvalidOperationException("AutoLoad already loaded");
        _instance = this;

        Assembly[] assemblies =
        [
            Assembly.Load("NodeImplementations"),
            Assembly.Load("IO")
        ];

        foreach (var assembly in assemblies)
        {
            GraphNodeTypes.RegisterCurrentAssembly(assembly);
            LogLady.Debug($"Registered assembly: {assembly}");
        }

        LogLady.AddLogger<GDLogger>();
        LogLady.AddLogger<ConsoleLogger>();
    }

    public override void _Process(double dt)
    {
        if (_isPaused)
            return;

        ProcessLoop.AllowRunOnce();
        UiRunner.Do();
    }

    private bool _isPaused;

    ~AutoLoad()
    {
        if (_instance != this)
            return;

        _instance = null;
        LogLady.RemoveAllLoggers();
        ProcessLoop.Stop();
    }
}