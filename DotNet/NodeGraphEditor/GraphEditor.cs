using System;
using Godot;
using NodeGraphEditor.GraphNodes;
using NodeGraphEditor.NodeImplementations;
using OperatorCore;

namespace NodeGraphEditor.Editor;

public partial class GraphEditor : GraphEdit
{
    [Export] private PackedScene? _template;

    public override void _Ready()
    {
        base._Ready();
        //BeginNodeMove += _OnBeginNodeMove;
        //EndNodeMove += _OnEndNodeMove;
        //NodeSelected += _OnNodeSelected;
        //NodeDeselected += _OnNodeDeselected;
        //
        //ConnectionRequest += _OnConnectionRequest;
        //DisconnectionRequest += _OnDisconnectionRequest;
        //ConnectionToEmpty += _OnConnectionToEmpty;
        //ConnectionFromEmpty += _OnConnectionFromEmpty;
        //ConnectionDragStarted += _OnConnectionDragStarted;
        //ConnectionDragEnded += _OnConnectionDragEnded;
        //
        //CopyNodesRequest += _OnCopyNodesRequest;
        //PasteNodesRequest += _OnPasteNodesRequest;
        //DeleteNodesRequest += _OnDeleteNodesRequest;
        //DuplicateNodesRequest += _OnDuplicateNodesRequest;;
        //
        //PopupRequest += _OnPopupRequest;
        //ScrollOffsetChanged += _OnScrollOffsetChanged;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        NodeGraphEditor.CustomGraphNode? node = null;
        GraphNodeLogic? nodeLogic = null;
        if (@event is InputEventKey { Pressed: true } key)
        {
            switch (key.Keycode)
            {
                case Key.Pageup:
                    node = Instantiate();
                    nodeLogic = new CountSeconds
                    {
                        InstanceId = Guid.NewGuid()
                    };
                    break;
                case Key.Pagedown:
                    node = Instantiate();
                    nodeLogic = new TestNode
                    {
                        InstanceId = Guid.NewGuid()
                    };
                    break;
            }
        }
        else if (@event is InputEventMouseButton { Pressed: true } mouse)
        {
            if ((mouse.ButtonMask & MouseButtonMask.MbXbutton2) != 0)
            {
                // create a new node
                node = Instantiate();
                nodeLogic = new TestNode()
                {
                    InstanceId = Guid.NewGuid()
                };
            }
            else if ((mouse.ButtonMask & MouseButtonMask.MbXbutton1) != 0)
            {
                node = Instantiate();
                nodeLogic = new CountSeconds
                {
                    InstanceId = Guid.NewGuid()
                };
            }
        }

        if (nodeLogic != null)
        {
            node!.ApplyNode(nodeLogic);
            AddChild(node);

            var mousePos = GetViewport().GetMousePosition();
            node.PositionOffset = mousePos;
        }

        return;

        NodeGraphEditor.CustomGraphNode Instantiate()
        {
            return _template!.Instantiate<NodeGraphEditor.CustomGraphNode>();
        }
    }

    private void OnNodeCreated(NodeGraphEditor.CustomGraphNode node)
    {
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
}