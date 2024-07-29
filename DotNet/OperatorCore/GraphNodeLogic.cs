using System.Reflection;
using System.Runtime.CompilerServices;
using NodeGraphEditor.Editor;

[assembly: InternalsVisibleTo("NodeGraphEditor")]

namespace OperatorCore;

public abstract class GraphNodeLogic
{
    internal event Action? Destroyed;

    // this is a unique identifier for the instance of the node
    // it is an init property so it does not need to be set in the constructor,
    // which would be both annoying for child-classes and liable to modification by the child classes
    private Guid _instanceId = Guid.Empty;
    internal unsafe Guid InstanceId
    {
        get => _instanceId;
        set
        {
            if (_instanceId != Guid.Empty)
            {
                throw new InvalidOperationException("Instance ID can only be set once");
            }
            
            _instanceId = value;
            var byteSpan = new ReadOnlySpan<byte>(&value, sizeof(Guid));
            var key = Convert.ToBase64String(byteSpan); // save some space by converting to base64 (32 chars -> 22 chars)
            ReplaceChar(key, '/', '_'); // replace any '/' with '_' as '/' is not a valid character in node name
            StringKey = key;

            Init();
        }
    }

    private static unsafe void ReplaceChar(string str, char toReplace, char replaceWith)
    {
        // replace any '/' with '_'
        var length = str.Length;
        fixed (char* pKey = str)
        {
            for (var i = 0; i < length; i++)
            {
                if (pKey[i] == toReplace)
                    pKey[i] = replaceWith;
            }
        }
    }

    internal string StringKey { get; private set; }

    protected internal GraphNodeLogic()
    {
    }

    internal void SetReady()
    {
        if(StringKey == null)
            throw new InvalidOperationException("Instance ID must be set before calling SetReady");
        
        OnInitialize();
    }
    
    protected abstract void OnInitialize();
    public abstract void Process(double delta);
    
    protected abstract void OnDestroy();

    private void Init()
    {
        var nodeType = GetType();
        var typeInfo = TypeCache.GetTypeInfo(nodeType);
        var fields = typeInfo.Fields;

        var fieldLength = fields.Length;
        if (fieldLength == 0)
            return;

        for (int i = 0; i < fieldLength; i++)
        {
            var field = fields[i];
            CheckFieldForSlot(field);
        }

        return;

        void CheckFieldForSlot(FieldInfo field)
        {
            var fieldType = field.FieldType;
            if (fieldType.IsAssignableTo(typeof(IInputSlot)))
            {
                ValidateAndAdd(field, _inputSlots);
            }
            else if (fieldType.IsAssignableTo(typeof(IOutputSlot)))
            {
                ValidateAndAdd(field, _outputSlots);
            }
        }

        void ValidateAndAdd<T>(FieldInfo field, List<T> slots) where T : ISlot
        {
            if (!field.IsInitOnly)
            {
                throw new InvalidOperationException($"Slot {field.Name} must be readonly on {nodeType.Name}");
            }
                
            var value = field.GetValue(this);
            if (value is not T inputSlot)
            {
                throw new NullReferenceException($"Slot {field.Name} is null on {nodeType.Name}");
            }

            inputSlot.Name = field.Name;
            slots.Add(inputSlot);
        }
    }

    internal SlotInfoIO[] GetSlotDefinitions()
    {
        var inputCount = _inputSlots.Count;
        var outputCount = _outputSlots.Count;
        var maxCount = Math.Max(inputCount, outputCount);
        var slots = new SlotInfoIO[maxCount];

        for (int i = 0; i < maxCount; i++)
        {
            var inputDef = GetSlotInfo(i, _inputSlots);
            var outputDef = GetSlotInfo(i, _outputSlots);
            slots[i] = new SlotInfoIO(inputDef, outputDef);
        }

        return slots;

        static SlotInfo GetSlotInfo<T>(int i, List<T> slots) where T : ISlot
        {
            if(i >= slots.Count)
                return default;
            
            var slot = slots[i];
            return new SlotInfo(true, TypeCache.GetTypeInfo(slot.Type), slot);
        }
    }

    internal void Destroy()
    {
        if (_isDestroyed)
        {
            return;
        }

        _isDestroyed = true;
        Destroyed?.Invoke();
        OnDestroy();
    }
    

    private readonly List<IInputSlot> _inputSlots = new();
    private readonly List<IOutputSlot> _outputSlots = new();
    private bool _isDestroyed;
}

internal readonly record struct SlotInfoIO(SlotInfo Input, SlotInfo Output);
internal readonly record struct SlotInfo(bool Enable, TypeInfo TypeInfo, ISlot? Slot);

public readonly record struct TypeInfo(Type Type, int TypeIndex, FieldInfo[] Fields);