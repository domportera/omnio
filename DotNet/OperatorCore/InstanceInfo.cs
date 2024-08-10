using System.Text.Json.Serialization;

namespace OperatorCore;

[Serializable]
public record InstanceInfo(Guid TypeId, Guid InstanceId)
{
    // includes any typed-in values for this instance's inputs
    public string Rename = string.Empty;
    public string Note = string.Empty;
    
    [NonSerialized]
    public readonly Dictionary<ushort, InputValue> UnconnectedInputValues = new();

    void DoSom()
    {
        InstanceInfoJsonContext context;
    }
}

public abstract class InputValue
{
    
}

[Serializable]
public class InputValue<T> : InputValue
{
    public T Value;
}