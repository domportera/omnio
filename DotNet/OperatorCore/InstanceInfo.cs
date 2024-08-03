namespace OperatorCore;

public record InstanceInfo(Guid TypeId, Guid InstanceId)
{
    // includes any typed-in values for this instance's inputs

    public readonly Guid Id = InstanceId;
    public readonly Guid TypeId = TypeId;
    public string Rename = string.Empty;
    public string Note = string.Empty;
    public readonly Dictionary<ushort, InputValue> UnconnectedInputValues = new();
}

public abstract class InputValue
{
}

public class InputValue<T> : InputValue
{
}