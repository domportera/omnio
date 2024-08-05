using System.Diagnostics.CodeAnalysis;

namespace Utilities;

public interface ISerializer<in T>
{
    public byte[] Serialize(T obj);
}

public interface IDeserializer<T>
{
    public bool TryDeserialize(Span<byte> data, [NotNullWhen(true)] out T? obj);
}