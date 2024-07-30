using System;
using System.Collections.Generic;
using Godot;
using NodeGraphEditor.GraphNodeUI;
using NodeGraphEditor.UI;
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
        SetName(nodeLogic.InstanceIdString);
        SetTitle(nodeLogic.GetType().Name);
        var slotDefinitions = GetSlotDefinitions(_logic);
        SetSlots(slotDefinitions);
        _logic.SetReady();
    }

    public override void _Process(double delta)
    {
        // todo - move off the main thread ?
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
        
        
        var slotCount = slots.Length;
        
        for (var i = 0; i < slotCount; i++)
        {
            var ((inputEnable, (inputType, inputTypeIndex, _), inputSlot),
                (outputEnable, (outputType, outputTypeIndex, _), outputSlot)) = slots[i];

            unsafe
            {
                portCount += *(byte*)&inputEnable + *(byte*)&outputEnable;
            }

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
        PortUiGenerator.InitSlots(slots, this, _portControls);
    }

    internal static SlotInfoIO[] GetSlotDefinitions(GraphNodeLogic logic)
    {
        var inputSlots = logic.InputSlots;
        var outputSlots = logic.OutputSlots;
        var inputCount = inputSlots.Count;
        var outputCount = outputSlots.Count;
        var maxCount = Math.Max(inputCount, outputCount);
        var slots = new SlotInfoIO[maxCount];

        for (int i = 0; i < maxCount; i++)
        {
            var inputDef = GetSlotInfo(i, inputSlots);
            var outputDef = GetSlotInfo(i, outputSlots);
            slots[i] = new SlotInfoIO(inputDef, outputDef);
        }

        return slots;

        static SlotInfo GetSlotInfo<T>(int i, IReadOnlyList<T> slots) where T : ISlot
        {
            if (i >= slots.Count)
                return default;

            var slot = slots[i];
            return new SlotInfo(true, TypeCache.GetTypeInfo(slot.Type), slot);
        }
    }

    public override void _ExitTree()
    {
        _logic!.Destroy();
        base._ExitTree();
    }

    private List<PortControl>? _portControls;


    public void ReleaseUi()
    {
        
        foreach(var portControl in _portControls!)
        {
            portControl.Dispose();
        }
    }
}
internal readonly record struct SlotInfoIO(SlotInfo Input, SlotInfo Output);

internal readonly record struct SlotInfo(bool Enable, TypeInfo TypeInfo, ISlot? Slot);
