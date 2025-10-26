using System;
using System.Collections.Generic;
using System.Numerics;

namespace Worker369.Utility;

public sealed class FormatColumnSettings
{
    public readonly static FormatColumnSettings Instance = new();

    public IList<Type> RightAlignTypes { get; } = [];
    public string BorderStyle { get; set; }
    public string HeaderStyle { get; set; }

    private FormatColumnSettings() => Reset();

    public void Reset()
    {
        BorderStyle = "\x1b[2m";  // Dim
        HeaderStyle = "\x1b[32m"; // Green

        RightAlignTypes.Clear();
        Type[] types = [
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(BigInteger),
            typeof(double), typeof(decimal),
            typeof(bool), typeof(DateTime),
            typeof(NumberInfo), typeof(ByteInfo), typeof(Checkbox)
        ];
        Array.ForEach(types, RightAlignTypes.Add);
    }
}