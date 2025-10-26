using System;

namespace Worker369.Utility;

public readonly struct Checkbox : IComparable, IComparable<Checkbox>
{
    public bool?  IsChecked   { get; }
    public string Description { get; }
    public bool   IsPlainText { get; }

    public Checkbox(bool? isChecked)
    {
        IsChecked   = isChecked;
        Description = string.Empty;
        IsPlainText = false;
    }

    public Checkbox(bool? isChecked, string description)
    {
        IsChecked   = isChecked;
        Description = description;
        IsPlainText = false;
    }

    public Checkbox(bool? isChecked, string description, bool isPlainText)
    {
        IsChecked   = isChecked;
        Description = description;
        IsPlainText = isPlainText;
    }

    public int CompareTo(object obj)
    {
        if (obj is null) return 1;

        return obj is Checkbox other
            ? CompareTo(other)
            : throw new ArgumentException($"Object is not a {typeof(Checkbox)}");
    }

    public int CompareTo(Checkbox other)
    {
        var result = CompareIsChecked(other);
        return result == 0 ? CompareDescription(other) : result;
    }

    public override string ToString()
    {
        var style = Style.Instance;

        if (!IsChecked.HasValue)
            return IsPlainText
                ? $"[ ? ] {Description}".TrimEnd(' ')
                : $"{style.Dim}[ ? ] {Description}".TrimEnd(' ') + style.Reset;
        else
        {
            if (IsPlainText)
                return IsChecked.Value
                    ? $"[ ✓ ] {Description}".TrimEnd(' ')
                    : $"[   ] {Description}".TrimEnd(' ');
            else
                return IsChecked.Value
                    ? $"{style.Dim}[{style.Reset} ✓ {style.Dim}]{style.Reset} {Description}".TrimEnd(' ')
                    : $"{style.Dim}[   ] {Description}".TrimEnd(' ') + style.Reset;
        }
    }

    private int CompareIsChecked(Checkbox other)
    {
        var x = IsChecked;
        var y = other.IsChecked;

        if (x == y) return 0;

        if (!x.HasValue) return -1;        // null < false < true

        if (!y.HasValue) return 1;         // null < false < true

        return x.Value.CompareTo(y.Value);
    }

    private int CompareDescription(Checkbox other) => Description.CompareTo(other.Description);
}