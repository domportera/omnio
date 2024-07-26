using System.Reflection;
using System.Runtime.InteropServices;

namespace OperatorCore;

internal static class TypeCache
{
    public static Guid GetTypeGuid(Type type)
    {
        var attributes = type.GetCustomAttributes(typeof(GuidAttribute), false);
        if (attributes.Length == 0)
        {
            throw new InvalidOperationException($"Type {type.Name} must have a GuidAttribute");
        }

        GuidAttribute? guidAttribute = null;
        foreach(var attribute in attributes)
        {
            if (attribute is GuidAttribute guid)
            {
                guidAttribute = guid;
                break;
            }
        }
        
        if(guidAttribute == null)
            throw new InvalidOperationException($"Type {type.Name} must have a GuidAttribute");
        
        return new Guid(guidAttribute.Value);
    }
    
    /// <summary>
    /// Ensures the type is registered and returns the type info.
    /// </summary>
    public static TypeInfo GetTypeInfo(Type type)
    {
        if (TypeInfoCache.TryGetValue(type, out var genericTypeInfo))
        {
            return genericTypeInfo;
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var index = Types.Count;
        var typeInfo = new TypeInfo(type, index, fields);
        TypeInfoCache[type] = typeInfo;
        Types.Add(type);

        // make sure the types of the fields are also registered - for future slot expansion?
        foreach (var field in fields)
        {
            _ = GetTypeInfo(field.FieldType);
        }

        return typeInfo;
    }

    // because godot passes connection types as ints, we need to handle this in an odd way
    private static readonly Dictionary<Type, TypeInfo> TypeInfoCache = new(512);
    private static readonly List<Type> Types = new(512);
}