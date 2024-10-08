﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OperatorCore;

internal static class TypeCache
{
    internal static TypeAttributes GetTypeAttributes(Type type)
    {
        var attributes = type.GetCustomAttributes(inherit: false);
        if (attributes.Length == 0)
        {
            throw new InvalidOperationException($"Type {type.Name} must have a GuidAttribute");
        }

        GuidAttribute? guidAttribute = null;
        DescriptionAttribute? descriptionAttribute = null;
        CategoryAttribute? categoryAttribute = null;
        var hidden = false;
        foreach (var attribute in attributes)
        {
            switch (attribute)
            {
                case GuidAttribute guid:
                    guidAttribute = guid;
                    break;
                case DescriptionAttribute description:
                    descriptionAttribute = description;
                    break;
                case CategoryAttribute category:
                    categoryAttribute = category;
                    break;
                case HiddenAttribute _:
                    hidden = true;
                    break;
                default:
                    continue;
            }
        }

        if (guidAttribute == null)
            throw new InvalidOperationException($"Type {type.Name} must have a GuidAttribute");

        return new TypeAttributes(
            Guid: Guid.Parse(guidAttribute.Value),
            hidden,
            TypeDescription: descriptionAttribute?.Description ?? string.Empty,
            TypeCategory: categoryAttribute?.Category ?? string.Empty);
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
    private static readonly Dictionary<Type, TypeInfo> TypeInfoCache = new(256);
    private static readonly List<Type> Types = new(512);
    private static readonly Dictionary<Guid, Type> TypesByGuid = new(256);

    public static Type GetTypeById(Guid typeId)
    {
        if (!TypesByGuid.TryGetValue(typeId, out var type))
        {
            throw new InvalidOperationException($"Type with id {typeId} is not registered");
        }

        return type;
    }

    public static void RegisterType(Type type, Guid typeId)
    {
        TypesByGuid.Add(typeId, type);
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HiddenAttribute : Attribute
{
}

public readonly record struct TypeAttributes(Guid Guid, bool Hidden, string TypeDescription, string TypeCategory);
public readonly record struct TypeInfo(Type Type, int TypeIndex, FieldInfo[] Fields);

public static class GraphNodeTypes
{
    [RequiresUnreferencedCode("This method is used to register types at runtime")]
    public static void RegisterCurrentAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            if (type.IsAssignableTo(typeof(GraphNodeLogic)))
            {
                RegisterGraphNodeType(type);
            }
        }
    }

    private static void RegisterGraphNodeType(Type type)
    {
        var typeInfo = TypeCache.GetTypeAttributes(type);
        TypeCache.RegisterType(type, typeInfo.Guid);
        
        if(!typeInfo.Hidden)
            NodeLogicAttributesByName.Add(type.FullName!, typeInfo);
    }

    public static IReadOnlyDictionary<string, TypeAttributes> LogicAttributesByName => NodeLogicAttributesByName;
    private static readonly Dictionary<string, TypeAttributes> NodeLogicAttributesByName = new(128);
}