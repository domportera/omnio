using System;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace NodeGraphEditor.Editor;

internal static class FromStringMethods
{
    public static bool TryGet<TInput>([NotNullWhen(true)] out FromStringMethod<TInput>? method)
    {
        if (Methods.TryGetValue(typeof(TInput), out var conversionDelegate))
        {
            method = (FromStringMethod<TInput>)conversionDelegate;
            return true;
        }

        method = null;
        return false;
    }

    private static readonly FrozenDictionary<Type, Delegate> Methods =
        new System.Collections.Generic.Dictionary<Type, Delegate>
        {
            {typeof(bool), new FromStringMethod<bool>((string str, out bool result) => bool.TryParse(str, out result))},
            {typeof(int), new FromStringMethod<int>((string str, out int result) => int.TryParse(str, out result))},
            {typeof(float), new FromStringMethod<float>((string str, out float result) => float.TryParse(str, out result))},
            {typeof(double), new FromStringMethod<double>((string str, out double result) => double.TryParse(str, out result))},
            {typeof(long), new FromStringMethod<long>((string str, out long result) => long.TryParse(str, out result))},
            {typeof(ulong), new FromStringMethod<ulong>((string str, out ulong result) => ulong.TryParse(str, out result))},
            {typeof(short), new FromStringMethod<short>((string str, out short result) => short.TryParse(str, out result))},
            {typeof(ushort), new FromStringMethod<ushort>((string str, out ushort result) => ushort.TryParse(str, out result))},
            {typeof(byte), new FromStringMethod<byte>((string str, out byte result) => byte.TryParse(str, out result))},
            {typeof(sbyte), new FromStringMethod<sbyte>((string str, out sbyte result) => sbyte.TryParse(str, out result))},
            {typeof(char), new FromStringMethod<char>((string str, out char result) => char.TryParse(str, out result))},
            {typeof(string), new FromStringMethod<string>((string str, out string result) => { result = str; return true; })},
        }.ToFrozenDictionary();

}

public delegate bool FromStringMethod<T>(string str, [NotNullWhen(true)] out T result);