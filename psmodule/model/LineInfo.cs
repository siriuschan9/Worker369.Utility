using System;

namespace Worker369.Utility;

public sealed class LineInfo :IComparable, IComparable<LineInfo>
{
    public long   Line    {get;}
    public string Content {get;}

    internal LineInfo(int line, string content)
    {
        Line = line;
        Content = content;
    }

    public override string ToString() => $"{Line}: {Content}";

    public int CompareTo(object obj)
    {
        if (obj is null) return 1;

        if (obj is not LineInfo other)
            throw new ArgumentException($"Object is not {typeof(LineInfo)}", nameof(obj));
        else
            return CompareTo(other);
    }

    public int CompareTo(LineInfo other)
    {
        if (other is null) return 1;

        return Line.CompareTo(other.Line);
    }
}