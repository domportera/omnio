using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Utilities;
using Utilities.Logging;

namespace OperatorCore;

public abstract partial class GraphNodeLogic
{
    internal static GraphNodeLogic CreateNodeLogic(Guid typeId)
    {
        var type = TypeCache.GetTypeById(typeId);
        return CreateNodeLogicFromType(type);

        static GraphNodeLogic CreateNodeLogicFromType(Type type) => LogicConstructorCache.GetDefaultConstructor(type)();
    }
    
    private void SetReady()
    {
        if (InstanceIdString == null)
            throw new InvalidOperationException("Instance ID must be set before calling SetReady");

        OnInitialize();
        ProcessLoop.Add(this);
    }

    private void Init()
    {
        var nodeType = GetType();
        var typeInfo = TypeCache.GetTypeInfo(nodeType);
        var fields = typeInfo.Fields;

        var fieldLength = fields.Length;
        if (fieldLength == 0)
            return;

        HashSet<ushort> inputIds = new();
        HashSet<ushort> outputIds = new();
        for (int i = 0; i < fieldLength; i++)
        {
            var field = fields[i];
            if (!CheckFieldForSlot(field, out var id, out var type))
                continue;
            
            var ids = type switch 
            {
                SlotType.Input => inputIds,
                SlotType.Output => outputIds,
                _ => throw new InvalidOperationException($"Slot type {type} not yet supported")
            };

            if (!ids.Add(id.Value))
            {
                throw new InvalidOperationException($"Duplicate slot ID {id} found on {nodeType.Name}");
            }
        }

        CreateTransformationSlot(outGenericType: typeof(OutputSlot<>),
            originals: _inputSlots,
            generated: _defaultInputToOutputs,
            slotGenericArgs: _inputSlotGenericType,
            generatedTypeMap: OutputSlotTypeMap,
            constructorCache: OutputConstructorCache);

        CreateTransformationSlot(
            outGenericType: typeof(InputSlot<>),
            originals: _outputSlots,
            generated: _defaultOutputToInputs,
            slotGenericArgs: _outputSlotGenericType,
            generatedTypeMap: InputSlotTypeMap,
            constructorCache: InputConstructorCache);

        return;

        bool CheckFieldForSlot(FieldInfo field, [NotNullWhen(true)] out ushort? id, [NotNullWhen(true)] out SlotType? type)
        {
            var fieldType = field.FieldType;
            if (fieldType.IsAssignableTo(typeof(IInputSlot)))
            {
                id = ValidateAndAdd(field, _inputSlots, _inputSlotGenericType, InputSlotTypeMap);
                type = SlotType.Input;
                return true;
            }

            if (fieldType.IsAssignableTo(typeof(IOutputSlot)))
            {
                id = ValidateAndAdd(field, _outputSlots, _outputSlotGenericType, OutputSlotTypeMap);
                type = SlotType.Output;
                return true;
            }

            id = null;
            type = null;
            return false;

            if (fieldType.IsAssignableTo(typeof(IList<IInputSlot>)))
            {
                type = SlotType.InputList;
            }
            else if (fieldType.IsAssignableTo(typeof(IList<IOutputSlot>)))
            {
                type = SlotType.OutputList;
            }
        }

        ushort ValidateAndAdd<T>(FieldInfo field, List<T> slots, List<Type> genericTypes,
            Dictionary<Type, Type> typeMap) where T : ISlot
        {
            if (!field.IsInitOnly)
            {
                throw new InvalidOperationException($"Slot {field.Name} must be readonly on {nodeType.Name}");
            }

            var value = field.GetValue(this);
            if (value is not T slot)
            {
                throw new NullReferenceException($"Slot {field.Name} is null on {nodeType.Name}");
            }

            slot.Name = field.Name;
            slots.Add(slot);

            var slotType = field.FieldType;
            var genericTypeArg = slotType.GenericTypeArguments[0];
            genericTypes.Add(genericTypeArg);
            typeMap.TryAdd(genericTypeArg, slotType);
            return slot.Id;
        }
    }

    private static void CreateTransformationSlot<TIn, TOut>(Type outGenericType, List<TIn> originals,
        List<TOut> generated,
        List<Type> slotGenericArgs, Dictionary<Type, Type> generatedTypeMap,
        DynamicConstructorCache<TOut> constructorCache)
        where TIn : ISlot
        where TOut : ISlot
    {
#if DEBUG
        if (typeof(TIn) == typeof(TOut))
            throw new InvalidOperationException("Cannot mimic a slot with the same type");

        if (!outGenericType.ContainsGenericParameters)
            throw new InvalidOperationException("Generic type must be generic and undefined");

        var interfaces = outGenericType.GetInterfaces();
        if (!interfaces.Contains(typeof(ISlot)))
        {
            throw new InvalidOperationException($"Generic type must implement the {typeof(ISlot)} interface");
        }
#endif


        for (int i = 0; i < originals.Count; i++)
        {
            // create "barrier" slots to allow an input slot to be connected to nodes inside the current node
            var inputSlot = originals[i];
            var genericArg = slotGenericArgs[i];

            if (!generatedTypeMap.TryGetValue(genericArg, out var slotType))
            {
                // create type
                slotType = outGenericType.MakeGenericType(genericArg);
                generatedTypeMap.TryAdd(genericArg, slotType);
            }

            var constructor = constructorCache.GetDefaultConstructor(slotType);
            var slot = constructor();

            slot.ActAsTransformationSlot(inputSlot);
            generated.Add(slot);
        }
    }


    internal void ApplyRuntimeInfo(SubGraph subGraph, InstanceInfo instanceInfo)
    {
        SubGraph = subGraph;
        InstanceId = instanceInfo.InstanceId;
        foreach (var kvp in instanceInfo.UnconnectedInputValues)
        {
            var potentialSlot = _inputSlots.Find(slot => slot.Id == kvp.Key);

            if (potentialSlot == null)
            {
                LogLady.Error($"Failed to find slot with ID {kvp.Key}");
                continue;
            }

            potentialSlot.ApplyInputValue(kvp.Value);
        }

        Init();
        SetReady();
    }

    private readonly List<Type> _inputSlotGenericType = new();
    private readonly List<Type> _outputSlotGenericType = new();
    private static readonly Dictionary<Type, Type> InputSlotTypeMap = new();
    private static readonly Dictionary<Type, Type> OutputSlotTypeMap = new();
    private static readonly DynamicConstructorCache<IOutputSlot> OutputConstructorCache = new();
    private static readonly DynamicConstructorCache<IInputSlot> InputConstructorCache = new();
    private static readonly DynamicConstructorCache<GraphNodeLogic> LogicConstructorCache = new();
    private enum SlotType { Input, Output, InputList, OutputList }
}