using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using NodeGraphEditor.NodeImplementations;
using OperatorCore;

namespace NodeGraphEditor.Editor;

public partial class GraphEditor : GraphEdit
{
    [Export] private PackedScene? _template;
    [Export] private TypeInSearch _typeInSearch;

    public override void _Ready()
    {
        base._Ready();
        RightDisconnects = true;

        //BeginNodeMove += _OnBeginNodeMove;
        //EndNodeMove += _OnEndNodeMove;
        //NodeSelected += _OnNodeSelected;
        //NodeDeselected += _OnNodeDeselected;
        //
        ConnectionRequest += _OnConnectionRequest;
        DisconnectionRequest += _OnDisconnectionRequest;
        //ConnectionToEmpty += _OnConnectionToEmpty;
        //ConnectionFromEmpty += _OnConnectionFromEmpty;
        //ConnectionDragStarted += _OnConnectionDragStarted;
        //ConnectionDragEnded += _OnConnectionDragEnded;
        //
        //CopyNodesRequest += _OnCopyNodesRequest;
        //PasteNodesRequest += _OnPasteNodesRequest;
        DeleteNodesRequest += _OnDeleteNodesRequest;
        //DuplicateNodesRequest += _OnDuplicateNodesRequest;;
        //
        //PopupRequest += _OnPopupRequest;
        //ScrollOffsetChanged += _OnScrollOffsetChanged;

        _typeInSearch.SetItems(() => TypeCache.GraphNodeLogicTypes.Values
            .Select(x => x.FullName)
            .Where(x => x != null)!);

        _typeInSearch.ItemSelected += OnTypeSelected;
    }

    private void OnTypeSelected(object? _, string selectedTypeName)
    {
        var type = TypeCache.GraphNodeLogicTypes[selectedTypeName];
        CreateNodeOfType(type);
        CloseTypeInSearch(_typeInSearch);
    }

    private static void CloseTypeInSearch(TypeInSearch typeInSearch)
    {
        typeInSearch.Visible = false;
        typeInSearch.ReleaseFocus();
        typeInSearch.Text = "";
    }

    private void CreateNodeOfType(Type type)
    {
        var node = Instantiate();
        var constructor = NodeConstructorCache.GetDefaultConstructor(type);
        
        GraphNodeLogic nodeLogic;
        try
        {
            nodeLogic = constructor();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to create node of type {type.FullName}: {e}");
            return;
        }

        nodeLogic.InstanceId = Guid.NewGuid();
        node.ApplyNode(nodeLogic);
        AddChild(node);
        OnNodeCreated(node, nodeLogic);
    }

    private void _OnDeleteNodesRequest(Godot.Collections.Array nodes)
    {
        Console.WriteLine($"Deleting {nodes.Count} nodes");
        foreach (var node in nodes)
        {
            if (node.Obj is not StringName name) continue;

            if (!_nodes.Remove(name.ToString(), out var customNode))
            {
                Console.WriteLine($"Node '{name}' not found");
                continue;
            }

            customNode.ReleaseGraphNode();
            customNode.QueueFree();
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey { Pressed: true } key)
        {
            switch (key.Keycode)
            {
                case Key.Tab:
                    if (!_typeInSearch.Visible)
                    {
                        _typeInSearch.Visible = true;
                        _typeInSearch.GrabFocus();
                    }
                    else
                    {
                        CloseTypeInSearch(_typeInSearch);
                    }

                    break;
            }
        }
    }

    private CustomGraphNode Instantiate() => _template!.Instantiate<CustomGraphNode>();

    private void OnNodeCreated(CustomGraphNode node, GraphNodeLogic nodeLogic)
    {
        var mousePos = GetViewport().GetMousePosition();
        node.PositionOffset = mousePos;
        _nodes.Add(nodeLogic.StringKey, node);
    }

    // todo: allow dynamic type checking
    private void _OnConnectionRequest(StringName fromNodeName, long fromPortIndex, StringName toNodeName,
        long toPortIndex)
    {
        if (!TryGetFromToPorts(fromNodeName, fromPortIndex, toNodeName, toPortIndex, out var fromSlot, out var toSlot))
            return;

        if (toSlot.TryConnectTo(fromSlot))
        {
            ConnectNode(fromNodeName, (int)fromPortIndex, toNodeName, (int)toPortIndex);
        }
    }

    private readonly record struct ConnectionRequestData();

    private bool TryGetFromToPorts(StringName fromNodeName, long fromPortIndex, StringName toNodeName, long toPortIndex,
        [NotNullWhen(true)] out IOutputSlot? fromSlot,
        [NotNullWhen(true)] out IInputSlot? toSlot)
    {
        var fromNodeNameStr = fromNodeName.ToString();
        if (!_nodes.TryGetValue(fromNodeNameStr, out var fromNode))
        {
            Console.WriteLine($"Node '{fromNodeNameStr}' not found");
            fromSlot = null;
            toSlot = null;
            return false;
        }

        var toNodeNameStr = toNodeName.ToString();
        if (!_nodes.TryGetValue(toNodeNameStr, out var toNode))
        {
            Console.WriteLine($"Node '{toNodeNameStr}' not found");
            fromSlot = null;
            toSlot = null;
            return false;
        }

        fromSlot = fromNode.GetOutputPort((int)fromPortIndex);
        toSlot = toNode.GetInputPort((int)toPortIndex);
        return true;
    }


    private void _OnDisconnectionRequest(StringName fromNodeName, long fromPortId, StringName toNodeName, long toPortId)
    {
        if (!TryGetFromToPorts(fromNodeName, fromPortId, toNodeName, toPortId, out _, out var toSlot))
            return;

        toSlot.ReleaseConnection();
        DisconnectNode(fromNodeName, (int)fromPortId, toNodeName, (int)toPortId);
    }

    //public override void _Process(double delta)
    //{
    //    base._Process(delta);
    //    //if()
    //}
    //public override Vector2[] _GetConnectionLine(Vector2 fromPosition, Vector2 toPosition)
    //{
    //    return base._GetConnectionLine(fromPosition, toPosition);
    //}

    //public override bool _IsInInputHotzone(GodotObject inNode, int inPort, Vector2 mousePosition)
    //{
    //    return base._IsInInputHotzone(inNode, inPort, mousePosition);
    //}

    //public override bool _IsInOutputHotzone(GodotObject inNode, int inPort, Vector2 mousePosition)
    //{
    //    return base._IsInOutputHotzone(inNode, inPort, mousePosition);
    //}

    //public override bool _IsNodeHoverValid(StringName fromNode, int fromPort, StringName toNode, int toPort)
    //{
    //    return base._IsNodeHoverValid(fromNode, fromPort, toNode, toPort);
    //}
    //
    //protected override bool HasGodotClassSignal(in godot_string_name signal)
    //{
    //    return base.HasGodotClassSignal(in signal);
    //}

    private readonly Dictionary<string, CustomGraphNode> _nodes = new();
    private static readonly DynamicConstructorCache<GraphNodeLogic> NodeConstructorCache = new();
}