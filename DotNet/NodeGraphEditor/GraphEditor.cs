using System;
using System.Collections.Generic;
using Godot;
using OperatorCore;
using Utilities.Logging;

namespace NodeGraphEditor;

public partial class GraphEditor : GraphEdit
{
    [Export] private PackedScene? _template;
    [Export] private UI.TypeInSearch? _typeInSearch;

    private Vector2 _placementPosition;

    public void SetRootNode(GraphNodeLogic node)
    {
        _currentRoot = node;
    }

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

        _typeInSearch!.SetItems(() => GraphNodeTypes.LogicAttributesByName.Keys);
        
        _typeInSearch.Visible = false;

        _typeInSearch.ItemSelected += OnTypeSelected;


        // todo - actually load specific types based on user selection or whatever
        var instanceInfo = new InstanceInfo(Guid.Empty, Guid.NewGuid());
        if (!SubGraph.TryCreateInstance(instanceInfo, out var logic))
        {
            throw new InvalidOperationException("Failed to create root node");
        }
        
        SetRootNode(logic);
    }

    private void OnTypeSelected(object? _, string selectedTypeName)
    {
        CreateNodeOfType(selectedTypeName);
        CloseTypeInSearch(_typeInSearch!);
    }

    private static void CloseTypeInSearch(UI.TypeInSearch typeInSearch)
    {
        typeInSearch.Visible = false;
        typeInSearch.ReleaseFocus();
        typeInSearch.Text = "";
    }

    private void CreateNodeOfType(string typeName)
    {
        var typeAttributes = GraphNodeTypes.LogicAttributesByName[typeName];
        if (!_currentRoot.SubGraph.TryCreateNewNodeLogic(typeAttributes.Guid, out var nodeLogic))
        {
            LogLady.Error($"Failed to create node of type {typeName}");
            return;
        }

        var node = _template!.Instantiate<CustomGraphNode>();
        node.ApplyLogic(nodeLogic);
        AddChild(node);

        node.PositionOffset = (_placementPosition + ScrollOffset) / Zoom;
        _subGraphUi.NodeUIs.Add(nodeLogic.InstanceId, node);
    }

    private void _OnDeleteNodesRequest(Godot.Collections.Array nodes)
    {
        LogLady.Info($"Deleting {nodes.Count} nodes");
        foreach (var node in nodes)
        {
            if (node.Obj is not StringName name) continue;

            var nameStr = name.ToString();
            var guid = Guid.Parse(nameStr);

            if(!_currentRoot.SubGraph.RemoveNode(guid))
            {
                LogLady.Error($"Node '{nameStr}' not found");
                continue;
            }

            var customNode = _subGraphUi.NodeUIs[guid];
            customNode.ReleaseUi();
            customNode.QueueFree();
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        // hotkeys
        if (@event is InputEventKey { Pressed: true } key)
        {
            switch (key.Keycode)
            {
                case Key.Tab:
                    ToggleNodeSearch();
                    break;
            }
        }
    }

    private void ToggleNodeSearch()
    {
        // if mouse is not over the graph, don't show the search
        var globalMousePosition = GetGlobalMousePosition();
        if (!GetGlobalRect().HasPoint(globalMousePosition))
            return;
        
        if (!_typeInSearch!.Visible)
        {
            _typeInSearch.GlobalPosition = globalMousePosition;
            _typeInSearch.Visible = true;
            _placementPosition = globalMousePosition;
        }
        else
        {
            CloseTypeInSearch(_typeInSearch);
        }
    }


    // todo: allow dynamic type checking
    private void _OnConnectionRequest(StringName fromNodeName, long fromPortIndex, StringName toNodeName,
        long toPortIndex)
    {
        var fromPort = CreatePortInfo(fromNodeName, fromPortIndex);
        var toPort = CreatePortInfo(toNodeName, toPortIndex);
        if (_currentRoot.TryAddConnection(fromPort, toPort))
        {
            ConnectNode(fromNodeName, (int)fromPortIndex, toNodeName, (int)toPortIndex);
        }
    }    

    private void _OnDisconnectionRequest(StringName fromNodeName, long fromPortIndex, StringName toNodeName, long toPortIndex)
    {
        var fromPort = CreatePortInfo(fromNodeName, fromPortIndex);
        var toPort = CreatePortInfo(toNodeName, toPortIndex);
        if (_currentRoot.RemoveConnection(fromPort, toPort))
        {
            DisconnectNode(fromNodeName, (int)fromPortIndex, toNodeName, (int)toPortIndex);
        }
    }
    
    private static RuntimePortInfo CreatePortInfo(StringName nodeName, long portIndex) =>
        new(Guid.Parse(nodeName), (int)portIndex); 

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

    private GraphNodeLogic _currentRoot;
    private readonly SubGraphUi _subGraphUi = new();
}

[Serializable]
internal class SubGraphUi
{
    [NonSerialized]
    public readonly Dictionary<Guid, CustomGraphNode> NodeUIs = new();
    
    // instance Id is the key
    public readonly Dictionary<Guid, GraphNodeUi> NodeUis = new();
}

[Serializable]
internal struct GraphNodeUi
{
    
}