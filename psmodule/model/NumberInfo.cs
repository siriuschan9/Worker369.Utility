using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Worker369.Utility;

public class NumberInfo : IFormattable, IComparable, IComparable<NumberInfo>
{
    private static readonly IUnitsProvider _names_provider         = UnitsProvider.NumericUnitNames;
    private static readonly IUnitsProvider _abbreviations_provider = UnitsProvider.NumericUnitAbbreviations;

    private static readonly IEnumerable<string> _names         = _names_provider.GetUnits();
    private static readonly IEnumerable<string> _abbreviations = _abbreviations_provider.GetUnits();

    private static readonly NumberInfoSettings _default_settings = NumberInfoSettings.Instance;

    private readonly BigInteger         _value;
    private          NumberInfoSettings _my_settings;
    private          string             _display_unit;

    public BigInteger Value => _value;

    public string DisplayUnit
    {
        get { return _display_unit; }
        set { _display_unit = value; }
    }

    public NumberInfoSettings FormatSettings
    {
        get { return _my_settings; }
        set { _my_settings = value; }
    }

    public NumberInfo(BigInteger value)
    {
        _value = value;
    }

    public NumberInfo(BigInteger value, NumberInfoSettings settings, string displayUnit)
    {
        _value        = value;
        _my_settings  = settings;
        _display_unit = displayUnit;
    }

    public override string ToString() => ToString(GetFormat(), CultureInfo.CurrentCulture);

    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty(format)) format = GetFormat();

        if (format.StartsWith("C"))
        {
            string unit = format.Remove(0, 1);
            return ToDisplayUnitString(unit);
        }

        switch (format)
        {
            case "N" : return ToNumericString();
            case "H" : return ToHumanReadableString();
            case "U" : return ToUserFriendlyString();
            default  : return ToNumericString();
        }
    }

    public string ToNumericString()
    {
        var settings = _my_settings ?? _default_settings;
        var format   = settings.Format.Unscaled;
        return _value.ToString(format);
    }

    public string ToHumanReadableString()
    {
        var settings      = _my_settings ?? _default_settings;
        var scaled_format = settings.Format.Scaled;
        var scaled_value  = _abbreviations_provider.Scale(_value, out string unit);

        return scaled_value.ToString(scaled_format) + unit;
    }

    public string ToUserFriendlyString()
    {
        var settings      = _my_settings ?? _default_settings;
        var scaled_format = settings.Format.Scaled;
        var scaled_value  = _names_provider.Scale(_value, out string unit);

        return (scaled_value.ToString(scaled_format) +  " " + unit).TrimEnd(' ');
    }

    public string ToDisplayUnitString(string unit)
    {
        var settings           = _my_settings ?? _default_settings;
        string scaled_format   = settings.Format.Scaled;
        string fraction_format = settings.Format.Fraction;

        decimal scaled_value;

        if (_names.Contains(unit, StringComparer.OrdinalIgnoreCase))
        {
            scaled_value = _names_provider.ScaleToUnit(_value, unit);

            return Math.Abs(scaled_value) < 1
                ? (scaled_value.ToString(fraction_format) + " " + unit).TrimEnd(' ')
                : (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd(' ');
        }

        if (_abbreviations.Contains(unit, StringComparer.OrdinalIgnoreCase))
        {
            scaled_value = _abbreviations_provider.ScaleToUnit(_value, unit);

            return Math.Abs(scaled_value) < 1
                ? (scaled_value.ToString(fraction_format) + unit).TrimEnd(' ')
                : (scaled_value.ToString(scaled_format) + unit).TrimEnd(' ');
        }

        return null;
    }

    public int CompareTo(object obj)
    {
        if (obj is null) return 1;

        return obj is NumberInfo other
            ? _value.CompareTo(other._value)
            : throw new ArgumentException($"Object is not {typeof(NumberInfo)}");
    }

    public int CompareTo(NumberInfo other)
    {
        if (other is null) return 1;

        return _value.CompareTo(other._value);
    }

    public override bool Equals(object obj)
    {
        return obj is NumberInfo other && _value.Equals(other._value);
    }

    public override int GetHashCode() => _value.GetHashCode();

    private string GetFormat()
    {
        if (_display_unit != null)
            return $"C{_display_unit}";

        var settings = _my_settings ?? _default_settings;

        switch (settings.DisplayStyle)
        {
            case NumberDisplay.None          : return "N";
            case NumberDisplay.HumanReadable : return "H";
            case NumberDisplay.UserFriendly  : return "U";
            default                          : return "N";
        }
    }
}