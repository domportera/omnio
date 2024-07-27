using System;
using System.Collections.Generic;
using Godot;
using NodeGraphEditor.GraphNodes;
using OperatorCore;
using Expression = System.Linq.Expressions.Expression;

namespace NodeGraphEditor.Editor;
internal static class PortUiGenerator
{
    internal static void InitSlots(SlotInfoIO[] slots, CustomGraphNode parent, GraphNodeLogic logic, List<PortControl> portControls)
    {
        // todo - it'd be nice to have a way to have a graph view in the middle of the input and output slots
        // this may not be possible with godot's current GraphNode system due to how the ports are drawn in pairs >_>
        parent.Resizable = true;

        for (int i = 0; i < slots.Length; i++)
        {
            var slotPair = slots[i];
            var ((inputEnabled, inputTypeInfo, inputSlot), (outputEnabled, outputTypeInfo, outputSlot)) = slotPair;

            if (!inputEnabled && !outputEnabled)
                continue;

            var row = new HBoxContainer();
            row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            row.Alignment = BoxContainer.AlignmentMode.Center;
            row.Name = $"Slot {i}";
            row.SetAnchorsPreset(Control.LayoutPreset.TopWide);

            if (inputEnabled)
            {
                var control = CreateControl(inputSlot!, inputTypeInfo, row, Control.LayoutPreset.LeftWide, Control.GrowDirection.Begin);
                portControls.Add(control);
            }
            else
            {
                _ = AddDummyTo(row, Control.LayoutPreset.LeftWide, Control.GrowDirection.Begin);
            }

            var separator = new VSeparator();
            separator.SetAnchorsPreset(Control.LayoutPreset.VcenterWide);
            separator.Name = "Separator";
            separator.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            row.AddChild(separator);

            if (outputEnabled)
            {
                var control = CreateControl(outputSlot!, outputTypeInfo, row, Control.LayoutPreset.RightWide, Control.GrowDirection.End);
                portControls.Add(control);
            }
            else
            {
                _ = AddDummyTo(row, Control.LayoutPreset.RightWide, Control.GrowDirection.End);
            }

            parent.AddChild(row);
        }

        return;

        static Control AddDummyTo(HBoxContainer row, Control.LayoutPreset layout, Control.GrowDirection end)
        {
            var dummy = new Container();
            dummy.SetAnchorsPreset(layout);
            dummy.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            dummy.GrowHorizontal = end;
            row.AddChild(dummy);
            return dummy;
        }

        static PortControl CreateControl(ISlot slot, TypeInfo inputTypeInfo,
            HBoxContainer row, Control.LayoutPreset layout, Control.GrowDirection growDirection)
        {
            var slotType = slot.GetType();
            Func<PortControl> constructor;
            switch (slot)
            {
                case IInputSlot:
                {
                    if (!InputConstructors.TryGetValue(slotType, out constructor!))
                    {
                        constructor = GenerateConstructor(slotType, true);
                        InputConstructors[slotType] = constructor;
                    }

                    break;
                }
                case IOutputSlot:
                {
                    if(!OutputConstructors.TryGetValue(slotType, out constructor!))
                    {
                        constructor = GenerateConstructor(slotType, false);
                        OutputConstructors[slotType] = constructor;
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException($"Unknown slot type {slotType}");
            }

            var portControl = constructor();
            portControl.SetSlot(slot);
            var godotControl = portControl.Control;
            godotControl.GrowHorizontal = growDirection;
            godotControl.SetAnchorsPreset(layout);
            row.AddChild(godotControl);
            return portControl;
        }

        static Func<PortControl> GenerateConstructor(Type slotType, bool input)
        {
            var genericType = input ? typeof(DefaultInPortControl<>) : typeof(DefaultOutPortControl<>);
            var constructedType = genericType.MakeGenericType(slotType.GetGenericArguments());
                
            // compile this as a constructor expression
            var expression = Expression.Lambda<Func<PortControl>>(Expression.New(constructedType)).Compile();
            return expression;
        }
    }

    private static readonly Dictionary<Type, Func<PortControl>> InputConstructors = new();
    private static readonly Dictionary<Type, Func<PortControl>> OutputConstructors = new();
}