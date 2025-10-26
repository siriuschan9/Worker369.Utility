using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Worker369.Utility;

public class IPv4SubnetMask : IComparable, IComparable<IPv4SubnetMask>
{
    protected static uint _bitmask = 0xffff_ffffu; // 32 bit bitmask to aid binary arithmetics.

    protected readonly byte[]     _bytes = new byte[4]; // 4 byte array representation.
    protected          int        _prefix_length;       // Prefix length representation.
    protected          uint       _mask_value;          // Numeric value of the mask.
    protected          NumberInfo _size;                // Subnet size.

    public int PrefixLength
    {
        get { return _prefix_length; }
        set
        {
            if (value < 0 || value > 32)
                throw new ArgumentException("Prefix length must be between 0 and 32.");

            _prefix_length = value;

            UpdateBytes();     // 1. Update underlying bytes
            UpdateMaskValue(); // 2. Update mask value
            UpdateSize();      // 3. Update subnet size
        }
    }

    public   byte[]     Bytes     => (byte[])_bytes.Clone();           // A copy of the underlying bytes.
    public   string     Mask      => ToString();                       // Dot-decimal representation of the mask.
    public   string     Bits      => ToEmphasizedBinary(string.Empty); // Binary representation of the mask.
    public   NumberInfo Size      => _size;                            // Subnet size.
    internal BigInteger MaskValue => _mask_value;                      // Numeric value of the mask.

    public IPv4SubnetMask(int prefixLength)
    {
        if (prefixLength < 0 || prefixLength > 32)
            throw new ArgumentException("Prefix length must be between 0 and 32.");

        _prefix_length = prefixLength;

        UpdateBytes();     // 1. Update underlying bytes
        UpdateMaskValue(); // 2. Update mask value
        UpdateSize();      // 3. Update subnet size
    }

    public override string ToString()
    {
        return string.Format("{0}.{1}.{2}.{3}", _bytes[0], _bytes[1], _bytes[2], _bytes[3]);
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
        // e.g. prefix length   = 18
        //      mask            = 11111111_11111111_11000000_00000000 (underscore is for visual aid)
        //      host bits       = 000000_00000000
        //      separator index = 6
        int first_host_separator_index = 8 - (_prefix_length % 8);

        // insert separators to network bits.
        for (int i = 8; i <= network_bits.Length && i < 32; i += 8 + separator.Length)
            network_bits = network_bits.Insert(i, separator);

        // insert separators to host bits.
        for (int j = first_host_separator_index; j < host_bits.Length; j += 8 + separator.Length)
            host_bits = host_bits.Insert(j, separator);

        string network_style = IPv4AddressSettings.Instance.NetworkBitsStyle;
        string host_style    = IPv4AddressSettings.Instance.HostBitsStyle;
        string reset_style   = Style.Instance.Reset;

        // return emphasized string
        return $"{network_style}{network_bits}{host_style}{host_bits}{reset_style}";
    }

    public int CompareTo(object obj)
    {
        if (obj is null) return 1; // Follow convention: non-null > null.

        if (!(obj is IPv4SubnetMask other))
            throw new ArgumentException($"Object is not {typeof(IPv4SubnetMask)}");
        else
            return CompareTo(other); // Delegate to IComparable<T>.
    }

    public int CompareTo(IPv4SubnetMask other)
    {
        if (other is null) return 1; // Follow convention: non-null > null

        byte[] this_bytes  = _bytes;
        byte[] other_bytes = other.Bytes;

        for (int i = 0; i < 4; i++)
        {
            if (this_bytes[i] > other_bytes[i]) return 1;
            if (this_bytes[i] < other_bytes[i]) return -1;
        }
        return 0;
    }

    /// <summary>
    /// Returns an array of all 33 IPv4SubnetMask objects from prefix length /0 to /32 (0.0.0.0 to 255.255.255.255)
    /// </summary>
    public static IPv4SubnetMask[] ListAll()
    {
        var masks = new List<IPv4SubnetMask>();

        for (int i = 0; i <= 32; i++)
            masks.Add(new IPv4SubnetMask(i));

        return masks.ToArray();
    }

    // To invoke when _prefix_length changes. |  UpdateBytes() > UpdateMaskValue() > UpdateSize()
    protected void UpdateBytes_V2()
    {
        uint bits          = _bitmask;            // 11111111 11111111 11111111 11111111
        int  num_host_bits = 32 - _prefix_length; // number of bits to zero out.

        for (int i = 1; i <= num_host_bits; i++)  // zero out the bits from the right.
            bits <<= 1;

        for (int j = 0; j < _bytes.Length; j++)
            _bytes[j] = (byte)((bits >> (j * 8)) & 0xff);
    }

    // To invoke when _prefix_length changes. |  UpdateBytes() > UpdateMaskValue() > UpdateSize()
    protected void UpdateBytes()
    {
        // Set all bytes to 255 first.
        _bytes[0] = _bytes[1] = _bytes[2] = _bytes[3] = 255;

        // Short-circuit.
        if(_prefix_length == 32) return;

        for(int i = 3; i >= 0 ; i--) // walk the bytes from right to left.
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
            ? uint.MinValue
            : _bitmask << (32 - _prefix_length);
    }

    // To invoke when _prefix_length changes. |  UpdateBytes() > UpdateMaskValue() > UpdateSize()
    protected void UpdateSize()
    {
        uint       invert_mask_value = ~_mask_value & _bitmask;
        BigInteger subnet_size_value = (BigInteger)invert_mask_value + 1;

        _size = new NumberInfo(subnet_size_value);
    }
}