using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace NodeGraphEditor.UI;

internal static class ToStringMethods
{
    public static ToStringMethod<TInput> Get<TInput>()
    {
        if (Methods.TryGetValue(typeof(TInput), out var conversionDelegate))
            return (ToStringMethod<TInput>)conversionDelegate;

        return DefaultToString;
    }
    
    public static string DefaultToString<T>(in T value) =>
        value == null ? string.Empty : value.ToString() ?? string.Empty;

    private static readonly FrozenDictionary<Type, Delegate> Methods
        = new Dictionary<Type, Delegate>
            {
                { typeof(string), new ToStringMethod<string>((in string? val) => val ?? string.Empty) },
                { typeof(bool), new ToStringMethod<bool>((in bool val) => val.ToString()) },
                { typeof(int), new ToStringMethod<int>((in int val) => val.ToString(CultureInfo.InvariantCulture)) },
                {
                    typeof(float),
                    new ToStringMethod<float>((in float val) => val.ToString(CultureInfo.InvariantCulture))
                },
                {
                    typeof(double),
                    new ToStringMethod<double>((in double val) => val.ToString(CultureInfo.InvariantCulture))
                },
                { typeof(long), new ToStringMethod<long>((in long val) => val.ToString(CultureInfo.InvariantCulture)) },
                {
                    typeof(ulong),
                    new ToStringMethod<ulong>((in ulong val) => val.ToString(CultureInfo.InvariantCulture))
                },
                {
                    typeof(short),
                    new ToStringMethod<short>((in short val) => val.ToString(CultureInfo.InvariantCulture))
                },
                {
                    typeof(ushort),
                    new ToStringMethod<ushort>((in ushort val) => val.ToString(CultureInfo.InvariantCulture))
                },
                { typeof(byte), new ToStringMethod<byte>((in byte val) => val.ToString(CultureInfo.InvariantCulture)) },
                {
                    typeof(sbyte),
                    new ToStringMethod<sbyte>((in sbyte val) => val.ToString(CultureInfo.InvariantCulture))
                },
                { typeof(char), new ToStringMethod<char>((in char val) => val.ToString(CultureInfo.InvariantCulture)) },
                { typeof(Vector2), new ToStringMethod<Vector2>((in Vector2 val) => val.ToString()) },
                { typeof(Vector3), new ToStringMethod<Vector3>((in Vector3 val) => val.ToString()) },
                { typeof(Vector4), new ToStringMethod<Vector4>((in Vector4 val) => val.ToString()) },
                { typeof(Quaternion), new ToStringMethod<Quaternion>((in Quaternion val) => val.ToString()) },
            }
            .ToFrozenDictionary();
}
public delegate string ToStringMethod<TInput>(in TInput? value);
