using System.Collections.Generic;
using Godot;
using NodeGraphEditor.Editor;
using NodeGraphEditor.Engine;
using OperatorCore;

namespace NodeGraphEditor;

public sealed partial class CustomGraphNode : GraphNode
{
    private GraphNodeLogic? _logic;

    public void ApplyNode(GraphNodeLogic nodeLogic)
    {
        if(_logic != null)
            throw new System.InvalidOperationException("Node already has a logic definition");
        
        _logic = nodeLogic;
        SetName(_logic!.StringKey);
        SetTitle(_logic.GetType().Name);
        var slotDefinitions = _logic!.GetSlotDefinitions();
        SetSlots(slotDefinitions);
        _logic.SetReady();
    }

    public override void _Process(double delta)
    {
        _logic!.Process(delta);
    }

    //public override void _DrawPort(int slotIndex, Vector2I position, bool left, Color color)
    //{
    //    base._DrawPort(slotIndex, position, left, color);
    //    GD.Print($"Trying to draw slot {slotIndex} at {position}: ({(left ? "input" : "output")}) port with color {color}");
    //}

    private void SetSlots(SlotInfoIO[] slots)
    {
        var portCount = 0;
        var transparentColor = Colors.Transparent; // cache for performance;
        
        if(_inputSlots != null)
            throw new System.InvalidOperationException("Slots already set");
        
        var slotCount = slots.Length;
        _inputSlots = new IInputSlot[slotCount];
        _outputSlots = new IOutputSlot[slotCount];
        
        for (var i = 0; i < slotCount; i++)
        {
            var ((inputEnable, (inputType, inputTypeIndex, _), inputSlot),
                (outputEnable, (outputType, outputTypeIndex, _), outputSlot)) = slots[i];

            unsafe
            {
                portCount += *(byte*)&inputEnable + *(byte*)&outputEnable;
            }
            
            _inputSlots[i] = inputSlot as IInputSlot;
            _outputSlots[i] = outputSlot as IOutputSlot;

            SetSlot(slotIndex: i,
                enableLeftPort: inputEnable,
                typeLeft: inputTypeIndex,
                colorLeft: inputEnable ? TypeColors.GetFor(inputType) : transparentColor,
                enableRightPort: outputEnable,
                typeRight: outputTypeIndex,
                colorRight: outputEnable ? TypeColors.GetFor(outputType) : transparentColor
            );
        }

        _portControls = new List<PortControl>(portCount);
        PortUiGenerator.InitSlots(slots, this, _logic!, _portControls);
    }

    public override void _ExitTree()
    {
        _logic!.Destroy();
        base._ExitTree();
    }

    private List<PortControl>? _portControls;
    private IInputSlot?[]? _inputSlots;
    private IOutputSlot?[]? _outputSlots;
}