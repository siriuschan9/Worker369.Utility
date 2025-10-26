using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Worker369.Utility;

public class IPv6PrefixMask : IComparable, IComparable<IPv6PrefixMask>
{
    protected static BigInteger _bitmask = BigInteger.Pow(2, 128) -1 ; // 128 bit bitmask - all '1's

    protected readonly byte[]     _bytes = new byte[16]; // 16 byte array representation.
    protected          int        _prefix_length;        // Prefix length representation.
    protected          BigInteger _mask_value;           // Numeric value of the mask.
    protected          NumberInfo _size;                 // Subnet size.

    public int PrefixLength
    {
        get { return _prefix_length; }
        set
        {
            if (value < 0 || value > 128)
                throw new ArgumentException("Prefix length must be between 0 and 128.");

            _prefix_length = value;

            UpdateBytes();     // 1. Update underlying bytes
            UpdateMaskValue(); // 2. Update mask value
            UpdateSize();      // 3. Update subnet size
        }
    }

    public   byte[]     Bytes     => (byte[])_bytes.Clone();           // A copy of the underlying bytes.
    public   string     Bits      => ToEmphasizedBinary(string.Empty); // Binary representation of the mask.
    public   NumberInfo Size      => _size;                            // Subnet size.
    internal BigInteger MaskValue => _mask_value;                      // Numeric value of the mask.

    public IPv6PrefixMask(int prefixLength)
    {
        if (prefixLength < 0 || prefixLength > 128)
            throw new ArgumentException("Prefix length must be between 0 and 128.");

        _prefix_length = prefixLength;

        UpdateBytes();     // 1. Update underlying bytes
        UpdateMaskValue(); // 2. Update mask value
        UpdateSize();      // 3. Update subnet size
    }

    public override string ToString()
    {
        return string.Format("/{0}", _prefix_length);
    }

    public string ToBinary(string separator = " ")
    {
        string[] binary = _bytes.Select(octet => octet.ToString("b8")).ToArray();
        return string.Join(separator, binary);
    }

    public string ToEmphasizedBinary(string separator = " ")
    {
        // convert each byte to its binary form.
        string[] bytes_in_binary = _bytes.Select(octet => octet.ToString("b8")).ToArray();

        // save a 32-bit binary string of the IP.
        string binary = string.Join(string.Empty, bytes_in_binary);

        // save the network bits to a string variable.
        string network_bits = binary.Substring(0, _prefix_length);

        // save the host bits to a string variable.
        string host_bits = binary.Remove(0, _prefix_length);

        // save the index in the host bits string to insert the first separator to host bits.
        // i.e. prefix length   = 18
        //      mask            = 11111111_11111111_11000000_00000000 (underscore is for visual aid)
        //      host bits       = 000000_00000000
        //      separator index = 6
        var first_host_separator_index = 8 - (_prefix_length % 8);

        // insert separators to network bits.
        for (int i = 8; i <= network_bits.Length && i < 128; i += 8 + separator.Length)
            network_bits = network_bits.Insert(i, separator);

        // insert separators to host bits.
        for (int j = first_host_separator_index; j < host_bits.Length; j += 8 + separator.Length)
            host_bits = host_bits.Insert(j, separator);


        var network_style = IPv6AddressSettings.Instance.NetworkBitsStyle;
        var host_style    = IPv6AddressSettings.Instance.HostBitsStyle;
        var reset_style   = Style.Instance.Reset;

        // return emphasized string
        return $"{network_style}{network_bits}{host_style}{host_bits}{reset_style}";
    }

    public int CompareTo(object obj)
    {
        if (obj is null) return 1; // Follow convention: non-null > null.

        if (!(obj is IPv6PrefixMask other))
            throw new ArgumentException($"Object is not {typeof(IPv6PrefixMask)}");
        else
            return CompareTo(other); // Delegate to IComparable<T>.
    }

    public int CompareTo(IPv6PrefixMask other)
    {
        if (other is null) return 1; // Follow convention: non-null > null

        byte[] this_bytes  = _bytes;
        byte[] other_bytes = other.Bytes;

        for (int i = 0; i < _bytes.Length; i++)
        {
            if (this_bytes[i] > other_bytes[i]) return 1;
            if (this_bytes[i] < other_bytes[i]) return -1;
        }
        return 0;
    }

    /// <summary>
    /// Returns an array of all 129 IPv6Prefix objects from prefix length /0 to /129.
    /// </summary>
    public static IPv6PrefixMask[] ListAll()
    {
        var masks = new List<IPv6PrefixMask>();

        for (int i = 0; i <= 128; i++)
            masks.Add(new IPv6PrefixMask(i));

        return masks.ToArray();
    }

    // To invoke when _prefix_length changes. |  UpdateBytes() > UpdateMaskValue() > UpdateSize()
    protected void UpdateBytes_V2()
    {
        BigInteger bits          = _bitmask;             // 11111111 11111111 11111111 11111111
        int        num_host_bits = 128 - _prefix_length; // number of bits to zero out.

        for (int i = 1; i <= num_host_bits; i++)  // zero out the bits from the right.
            bits <<= 1;

        for (int j = 0; j < _bytes.Length; j++)
            _bytes[j] = (byte)((bits >> (j * 8)) & 0xff);
    }

    // To invoke when _prefix_length changes. |  UpdateBytes() > UpdateMaskValue() > UpdateSize()
    protected void UpdateBytes()
    {
        // Set all bytes to 255 first.
        for(int i = 0; i < _bytes.Length; i++) _bytes[i] = 255;

        // Short-circuit.
        if(_prefix_length == 128) return;

        for(int i = _bytes.Length - 1; i >= 0 ; i--) // walk the bytes from right to left.
        {
            if (_prefix_length < i * 8) // If prefix length is not in this byte,
                _bytes[i] = 0;          // zero all bits for this byte and move to the next one.
            else
            {
                _bytes[i] = (byte)(_bytes[i] << 8 - _prefix_length % 8); // zero out bits from the right.
                break;                                                   // break here.
            }
        }
    }

    // To invoke when _prefix_length changes. |  UpdateBytes() > UpdateMaskValue() > UpdateSize()
    protected void UpdateMaskValue()
    {
        _mask_value = _prefix_length == 0
            ? BigInteger.Zero
            : _bitmask << (128 - _prefix_length);
    }

    // To invoke when _prefix_length changes. |  UpdateBytes() > UpdateMaskValue() > UpdateSize()
    protected void UpdateSize()
    {
        BigInteger invert_mask_value = ~_mask_value & _bitmask;
        BigInteger subnet_size_value = invert_mask_value + 1;

        _size = new NumberInfo(subnet_size_value);
    }
}