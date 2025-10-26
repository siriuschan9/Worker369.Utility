using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Worker369.Utility;

public interface IUnitsProvider
{
    decimal             Scale       (BigInteger value, out string unit); // Auto-scale to the best unit.
    decimal             ScaleToUnit (BigInteger value, string unit);     // Manual-scale to a specific unit.
    IEnumerable<string> GetUnits();                                      // Get the list of available units.
}

public sealed class UnitsProvider : IUnitsProvider
{
    public readonly static UnitsProvider NumericUnitNames = new UnitsProvider(
        new string[]{
            string.Empty,
            "thousand",     //  3 zeros: k
            "million",      //  6 zeros: m
            "billion",      //  9 zeros: b
            "trillion",     // 12 zeros: t
            "quadrillion",  // 15 zeros: q
            "quintillion",  // 18 zeros: qi
            "sextillion",   // 21 zeros: sx
            "septillion",   // 24 zeros: sp
            "octillion",    // 27 zeros: o
            "nonillion",    // 30 zeros: n
            "decillion",    // 33 zeros: d
            "undecillion",  // 36 zeros: ud
            "duodecillion"  // 39 zeros: dd
        },
        Scaler.DecimalScaler);

    public readonly static UnitsProvider NumericUnitAbbreviations = new UnitsProvider(
        new string[]{
            string.Empty,
            "k",            //  3 zeros: thousand
            "m",            //  6 zeros: million
            "b",            //  9 zeros: billion
            "t",            // 12 zeros: trillion
            "q",            // 15 zeros: quadrillion
            "qi",           // 18 zeros: quintillion
            "sx",           // 21 zeros: sextillion
            "sp",           // 24 zeros: septillion
            "o",            // 27 zeros: octillion
            "n",            // 30 zeros: nonillion
            "d",            // 33 zeros: decillion
            "ud",           // 36 zeros: undecillion
            "dd"            // 39 zeros: duodecillion
        },
        Scaler.DecimalScaler);

    public readonly static UnitsProvider ByteUnitNames_SI = new UnitsProvider(
        new string[]{
            "Byte",
            "Kilobyte",  //  3 zeros
            "Megabyte",  //  6 zeros
            "Gigabyte",  //  9 zeros
            "Terabyte",  // 12 zeros
            "Petabyte",  // 15 zeros
            "Exabyte",   // 18 zeros
            "Zettabyte", // 21 zeros
            "Yottabyte"  // 24 zeros
        },
        Scaler.DecimalScaler);

    public readonly static UnitsProvider ByteUnitNames_IEC = new UnitsProvider(
        new string[]{
            "Byte",
            "Kibibyte", // 2^10
            "Mebibyte", // 2^20
            "Gibibyte", // 2^30
            "Tebibyte", // 2^40
            "Pebibyte", // 2^50
            "Exbibyte", // 2^60
            "Zebibyte", // 2^70
            "Yobibyte"  // 2^80
        },
        Scaler.BinaryScaler);

    public readonly static UnitsProvider ByteUnitAbbreviations_SI = new UnitsProvider(
        new string[]{
            "B",
            "KB", //  3 zeros
            "MB", //  6 zeros
            "GB", //  9 zeros
            "TB", // 12 zeros
            "PB", // 15 zeros
            "EB", // 18 zeros
            "ZB", // 21 zeros
            "YB", // 24 zeros
        },
        Scaler.DecimalScaler);

    public readonly static UnitsProvider ByteUnitAbbreviations_IEC = new UnitsProvider(
        new string[]{
            "B",
            "KiB", // 2^10
            "MiB", // 2^20
            "GiB", // 2^30
            "TiB", // 2^40
            "PiB", // 2^50
            "EiB", // 2^60
            "ZiB", // 2^70
            "YiB"  // 2^80
        },
        Scaler.BinaryScaler);

    private readonly string[] _units;
    private readonly Scaler   _scaler;

    internal UnitsProvider(string[] units, Scaler scaler){
        _units  = units;
        _scaler = scaler;
    }

    public decimal Scale(BigInteger value, out string unit)
    {
        var scaled_value = _scaler.Scale(value, out int order);

        if(order >= _units.Length) throw new OverflowException(
            $"Value is too large. Maximum unit that can be supported is {_units[_units.Length - 1]}");

        unit = _units[order];

        return scaled_value;
    }

    public decimal ScaleToUnit(BigInteger value, string unit)
    {
        var order = Array.FindIndex(_units, name => name.Equals(unit, StringComparison.InvariantCultureIgnoreCase));

        if (order == -1) throw new ArgumentException("Unit not found");

        return _scaler.ScaleToOrder(value, order);
    }

    public IEnumerable<string> GetUnits() => _units.AsEnumerable();
}