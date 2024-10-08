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

    public void ApplyLogic(GraphNodeLogic nodeLogic)
    {
        if (_logic != null)
            throw new System.InvalidOperationException("Node already has a logic definition");

        _logic = nodeLogic;
        SetName(nodeLogic.InstanceIdString);
        SetTitle(nodeLogic.GetType().Name);
        var slotDefinitions = GetSlotDefinitions(_logic);
        SetSlots(slotDefinitions, out var portCount);

        _portControls = new List<PortControl>(portCount);
        PortUiGenerator.InitSlots(slotDefinitions, this, _logic!, _portControls);
        

        return;

        static SlotInfoIO[] GetSlotDefinitions(GraphNodeLogic logic)
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
                return new SlotInfo(true, TypeCache.GetTypeInfo(slot.Type));
            }
        }
    }

    //public override void _DrawPort(int slotIndex, Vector2I position, bool left, Color color)
    //{
    //    base._DrawPort(slotIndex, position, left, color);
    //    GD.Print($"Trying to draw slot {slotIndex} at {position}: ({(left ? "input" : "output")}) port with color {color}");
    //}

    private void SetSlots(SlotInfoIO[] slots, out int portCount)
    {
        portCount = 0;
        var transparentColor = Colors.Transparent; // cache for performance;
        var slotCount = slots.Length;

        for (var i = 0; i < slotCount; i++)
        {
            var ((inputEnable, (inputType, inputTypeIndex, _)),
                (outputEnable, (outputType, outputTypeIndex, _))) = slots[i];

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
    }

    public override void _ExitTree()
    {
        _logic!.Destroy();
        base._ExitTree();
    }

    private List<PortControl>? _portControls;


    public void ReleaseUi()
    {
        if (_portControls == null)
            return;
        
        foreach (var portControl in _portControls)
        {
            portControl.Dispose();
        }
    }
}

internal readonly record struct SlotInfoIO(SlotInfo Input, SlotInfo Output);

internal readonly record struct SlotInfo(bool Enable, TypeInfo TypeInfo);