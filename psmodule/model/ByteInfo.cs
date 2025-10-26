using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Worker369.Utility;

public class ByteInfo : IFormattable, IComparable, IComparable<ByteInfo>
{
    private static readonly IUnitsProvider _si_names_provider          = UnitsProvider.ByteUnitNames_SI;
    private static readonly IUnitsProvider _si_abbreviations_provider  = UnitsProvider.ByteUnitAbbreviations_SI;
    private static readonly IUnitsProvider _iec_names_provider         = UnitsProvider.ByteUnitNames_IEC;
    private static readonly IUnitsProvider _iec_abbreviations_provider = UnitsProvider.ByteUnitAbbreviations_IEC;

    private static readonly IEnumerable<string> _si_names          = _si_names_provider.GetUnits();
    private static readonly IEnumerable<string> _si_abbreviations  = _si_abbreviations_provider.GetUnits();
    private static readonly IEnumerable<string> _iec_names         = _iec_names_provider.GetUnits();
    private static readonly IEnumerable<string> _iec_abbreviations = _iec_abbreviations_provider.GetUnits();

    private static readonly ByteInfoSettings _settings = ByteInfoSettings.Instance;

    private readonly BigInteger _value;
    private ByteMetric _metric_system;
    private string     _display_unit;

    public BigInteger Value
    {
        get { return _value; }
    }

    public string DisplayUnit
    {
        get { return _display_unit; }
        set { _display_unit = value; }
    }

    public ByteMetric MetricSystem
    {
        get {return _metric_system;}
        set { _metric_system = value;}
    }

    public ByteInfo(BigInteger value)
    {
        _value = value;
        _metric_system = ByteMetric.IEC;
    }

    public ByteInfo(BigInteger value, ByteMetric metricSystem)
    {
        _value = value;
        _metric_system = metricSystem;
    }

    public ByteInfo(BigInteger value, string displayUnit)
    {
        _value = value;
        _display_unit = displayUnit;
    }

    public override string ToString() => ToString(GetFormat(), CultureInfo.CurrentCulture);

    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty(format)) format = GetFormat();

        if (format.StartsWith('C'))
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
        var format = _settings.Format.Unscaled;
        return _value.ToString(format);
    }

    public string ToHumanReadableString()
    {
        string  scaled_format = _settings.Format.Scaled;
        decimal scaled_value;

        if (_metric_system == ByteMetric.IEC)
        {
            scaled_value  = _iec_abbreviations_provider.Scale(_value, out string unit);

            return (scaled_value.ToString(scaled_format) +  " " + unit).TrimEnd();
        }

        if (_metric_system == ByteMetric.SI)
        {
            scaled_value = _si_abbreviations_provider.Scale(_value, out string unit);

            return (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd();
        }

        return null;
    }

    public string ToUserFriendlyString()
    {
        string  scaled_format = _settings.Format.Scaled;
        decimal scaled_value;

        if (_metric_system == ByteMetric.IEC)
        {
            scaled_value = _iec_names_provider.Scale(_value, out string unit);

            return (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd(' ');
        }

        if (_metric_system == ByteMetric.SI)
        {
            scaled_value = _si_names_provider.Scale(_value, out string unit);

            return (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd(' ');
        }

        return null;
    }

    public string ToDisplayUnitString(string unit)
    {
        string scaled_format   = _settings.Format.Scaled;
        string fraction_format = _settings.Format.Fraction;

        decimal scaled_value;

        if (_iec_names.Contains(unit, StringComparer.OrdinalIgnoreCase))
        {
            scaled_value = _iec_names_provider.ScaleToUnit(_value, unit);

            return Math.Abs(scaled_value) < 1
                ? (scaled_value.ToString(fraction_format) + " " + unit).TrimEnd(' ')
                : (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd(' ');
        }

        if (_iec_abbreviations.Contains(unit, StringComparer.OrdinalIgnoreCase))
        {
            scaled_value = _iec_abbreviations_provider.ScaleToUnit(_value, unit);

            return Math.Abs(scaled_value) < 1
                ? (scaled_value.ToString(fraction_format) + " " + unit).TrimEnd(' ')
                : (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd(' ');
        }

        if (_si_names.Contains(unit, StringComparer.OrdinalIgnoreCase))
        {
            scaled_value = _si_names_provider.ScaleToUnit(_value, unit);

            return Math.Abs(scaled_value) < 1
                ? (scaled_value.ToString(fraction_format) + " " + unit).TrimEnd(' ')
                : (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd(' ');
        }

        if (_si_abbreviations.Contains(unit, StringComparer.OrdinalIgnoreCase))
        {
            scaled_value = _si_abbreviations_provider.ScaleToUnit(_value, unit);

            return Math.Abs(scaled_value) < 1
                ? (scaled_value.ToString(fraction_format) + " " + unit).TrimEnd(' ')
                : (scaled_value.ToString(scaled_format) + " " + unit).TrimEnd(' ');
        }

        return null;
    }

    public int CompareTo(object obj)
    {
        if (obj is null) return 1;

        return obj is ByteInfo other
            ? _value.CompareTo(other._value)
            : throw new ArgumentException("Object is not a ByteInfo");
    }

    public int CompareTo(ByteInfo other)
    {
        if (other is null) return 1;

        return _value.CompareTo(other._value);
    }

    public override bool Equals(object obj)
    {
        return obj is ByteInfo other && _value.Equals(other._value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    private string GetFormat()
    {
        if (_display_unit != null)
            return $"C{_display_unit}";

        switch (_settings.DisplayStyle)
        {
            case NumberDisplay.None          : return "N";
            case NumberDisplay.HumanReadable : return "H";
            case NumberDisplay.UserFriendly  : return "U";
            default                          : return "N";
        }
    }
}