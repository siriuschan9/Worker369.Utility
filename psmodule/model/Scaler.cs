using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Worker369.Utility;

internal sealed class Scaler
{
    internal readonly static Scaler DecimalScaler = new Scaler(1000);
    internal readonly static Scaler BinaryScaler  = new Scaler(1024);

    private static int _max_fractional_digits = 18; // Maximum number of fractional digits for numbers < 1.
    private static int _max_decimal_places = 9;     // Maximum number of decimal place for numbers >= 1.

    private static int _max_division_depth = Math.Max(_max_decimal_places, _max_fractional_digits);

    internal static int MaxFractionalDigits
    {
        get { return _max_fractional_digits;}
        set
        {
            if (value < 0) throw new ArgumentException("Number of fractional digits cannot be negative");
            if (value > 27) throw new ArgumentException("Number of fractional digits cannot exceed 27");

            _max_fractional_digits = value;
            _max_division_depth = Math.Max(_max_decimal_places, _max_fractional_digits);
        }
    }

    internal static int MaxDecimalPlace
    {
        get { return _max_decimal_places;}
        set
        {
            if (value < 0) throw new ArgumentException("Number of decimal places cannot be negative");
            if (value > 27) throw new ArgumentException("Number of decimal places cannot exceed 27");

            _max_decimal_places = value;
            _max_division_depth = Math.Max(_max_decimal_places, _max_fractional_digits);
        }
    }

    private readonly int _weight;

    internal Scaler(int baseWeight)
    {
        _weight = baseWeight;
    }

    internal int GetScalingOrder(BigInteger value)
    {
        int order = 0;

        while (value >= _weight)
        {
            value /= _weight;
            order++;
        }

        return order;
    }

    internal decimal Scale(BigInteger value, out int order)
    {
        // Scale the number down by orders of weight until it's less than weight.
        order = 0;

        // Work with the absolute value for scaling.
        BigInteger integral = BigInteger.Abs(value);

        // Finding the integral part and order of magnitude.
        while (integral >= _weight)
        {
            integral /= _weight;
            order++;
        }

        BigInteger abs = BigInteger.Abs(value);
        BigInteger pow = BigInteger.Pow(_weight, order);
        BigInteger rem = abs % pow;

        var fraction_list = new List<int>();
        Divide(rem, pow, fraction_list);

        // Round the fractional portion to the first two digits.
        int carry = RoundToNthFig(fraction_list, _max_decimal_places);   // e.g. 7.89

        // Add the carry to the integral, if any.
        integral += carry;

        // If the integral part reaches weight value, scale down again.
        if (integral == _weight)
        {
            integral = 1;
            order++;
        }

        // Get the fraction string and trim off insignificant zeros.
        var fraction_str = string.Join("",
            fraction_list.Select(digit => digit.ToString())).TrimEnd('0');

        // Append the fraction string with zeros if there is insufficient digits.
        while (fraction_str.Length < 2)
            fraction_str += '0';

        // 4 parts of the final string: [-][Integral].[Fraction] [Unit].
        string number_sign     = value < 0 ? "-" : string.Empty;
        string integral_part   = integral.ToString();
        string fractional_part = fraction_str;

        // Combine the parts and parse it as decimal.
        return decimal.Parse($"{number_sign}{integral_part}.{fractional_part}");
    }

    // Scale the number to a specific order of the base weight. i.e. 999 => 1000^2 => 0.000999
    internal decimal ScaleToOrder(BigInteger value, int order)
    {
        BigInteger abs = BigInteger.Abs(value);
        BigInteger pow = BigInteger.Pow(_weight, order);
        BigInteger rem = abs % pow;

        BigInteger integral = abs / pow;
        int num_frac_digits = integral < 1 ? _max_fractional_digits: _max_decimal_places;

        var fraction_list = new List<int>();
        Divide(rem, pow, fraction_list);

        // Perform rounding on the fractional part depending if integral is more or less than 1.
        int carry = integral < 1
            ? RoundToNthSigFig(fraction_list, num_frac_digits) // e.g. 0.123 456 789 123 456 789
            : RoundToNthFig(fraction_list, num_frac_digits);   // e.g. 7.123 456 789

        // Add the carry to the integral, if any.
        integral += carry;

        // If integral changes from 0 to 1, update the number of fractional digits to use values > 1.
        if (carry == 1 && integral == 1)
            num_frac_digits = _max_decimal_places;

        // Get the fraction string and trim off insignificant zeros.
        var fraction_str = string.Join("",
            fraction_list.Select(digit => digit.ToString())).TrimEnd('0');

        // Append the fraction string with zeros if there is insufficient digits.
        while (fraction_str.Length < num_frac_digits)
            fraction_str += '0';

        // 4 parts of the final string: [-][Integral].[Fraction] [Unit].
        string number_sign     = value < 0 ? "-" : string.Empty;
        string integral_part   = integral.ToString();
        string fractional_part = fraction_str;

        // Combine the parts and parse it as decimal.
        return decimal.Parse($"{number_sign}{integral_part}.{fractional_part}");
    }

    // For rounding numbers more than 1.
    private static int RoundToNthFig(List<int> digits, int n)
    {
        // Work with positive n only.
        if (n < 0) throw new ArgumentException("Parameter n cannot be less than zero.", nameof(n));

        // Ensure the list have enough digits.
        while (digits.Count < n + 1) digits.Add(0);

        // Set all non-consequential digits to zero.
        for (int i = n + 1; i < digits.Count; i++) digits[i] = 0;

        // If last consequential digit >= 5, initiate carry.
        int carry = digits[n] >= 5 ? 1 : 0;

        // Set the last consequential digit to 0 since we don't need it anymore now.
        digits[n] = 0;

        // Start the carry from the nth figure.
        int current = n - 1;

        // Continue the carry as long as the current digit is 9.
        while (carry == 1 && current >= 0)
        {
            if (digits[current] < 9)
            {
                digits[current] += 1;
                carry = 0;
                break;
            }
            else
            {
                digits[current] = 0;
                current--;
            }
        }

        return carry;
    }

    // For rounding numbers less than 1.
    private static int RoundToNthSigFig(List<int> digits, int n)
    {
        Debug.Assert(n > 0);

        // Ensure the list have enough digits.
        while (digits.Count < n + 1) digits.Add(0);

        // Short circuit.
        if (digits.All(d => d == 0)) return 0;

        // Calculate offset - the number of leading zeros.
        int offset = 0;
        int i = 0;
        while (i < digits.Count && digits[i++] == 0)
            offset++;

        // Add offset to n.
        n += offset;

        return RoundToNthFig(digits, n);
    }

    private static void Divide(BigInteger a, BigInteger b, List<int> fraction)
    {
        // Guard against divide by zero.
        if (b == 0) throw new DivideByZeroException();

        // Short-circuit.
        if (a == 0) return;

        // Work with absolute values only.
        a = BigInteger.Abs(a);
        b = BigInteger.Abs(b);

        // Work on the remainder.
        if (a > b) a %= b;

        // Borrow the first zero.
        a *= 10;

        BigInteger q, r;

        do
        {
            q = a / b;      // 0
            r = a % b;      // 200

            if (q == 0) // Not enough zero.
            {
                fraction.Add(0);  // Append one zero to the fraction.
                a *= 10;          // borrow one more zero.
            }
            else
            {
                Debug.Assert(q < 10);

                fraction.Add((int)q); // Append quotient to the fraction.
                a = r * 10;           // Continue divide the remainder.
            }

        } while (r != 0 && fraction.Count < _max_division_depth);
    }
}