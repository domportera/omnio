using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Godot;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace NodeGraphEditor.Engine;

internal static class TypeColors
{
    public static Color GetFor(Type type)
    {
        return ColorsByType.TryGetValue(type, out var color) 
            ? color 
            : new Color(0.35f, 0.2f, 0.2f);
    }

    private static readonly FrozenDictionary<Type, Color> ColorsByType = new Dictionary<Type, Color>()
    {
        {typeof(string), new Color(0, 1, 0)},
        {typeof(int), new Color(1, 0, 0)},
        {typeof(float), new Color(1, 0, 0)},
        {typeof(bool), new Color(1, 1, 0)},
        
        {typeof(Vector2), new Color(0, 0, 1)},
        {typeof(Vector3), new Color(0, 0, 0.8f)},
        {typeof(Vector4), new Color(0, 0, 0.5f)},
        {typeof(Color), new Color(0f, 0f, 0.5f)},
        {typeof(Quaternion), new Color(0f, 0f, 0.3f)}
    }.ToFrozenDictionary();
}