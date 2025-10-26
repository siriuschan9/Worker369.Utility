using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Worker369.Utility;

public class IPv4Address :IComparable, IComparable<IPv4Address>, IEquatable<IPv4Address>, IFormattable
{
    protected static readonly IPv4AddressSettings _settings = IPv4AddressSettings.Instance;

    protected readonly byte[]            _bytes;                        // Stored in network order - Big Endian
    protected readonly IPv4SubnetMask    _mask;

    public byte[] Bytes => (byte[])_bytes.Clone();                      // Returns a copy of the bytes.

    public string IPAddress => ToString(IPv4AddressDisplay.IP);         // By default, print out IP portion only.

    public IPv4SubnetMask SubnetMask => _mask;

    public int PrefixLength                                             // Use this to set the subnet mask.
    {
        get { return _mask.PrefixLength; }
        set { _mask.PrefixLength = value; }
    }

    public IPv4Address NetworkId
    {
        get
        {
            var network_bytes = new byte[4];        // Initialize a new byte array for the network ID.
            var ip_bytes      = _bytes;             // Byte representation of this IP.
            var mask_bytes    = _mask.Bytes;        // Byte representation of subnet mask.
            var prefix_length = _mask.PrefixLength; // Prefix length equivalent of the mask.

            // filter out network bits.
            for(int i = 0; i < 4; i++)
                network_bytes[i] = (byte)(ip_bytes[i] & mask_bytes[i]);

            // return network ID.
            return new IPv4Address(network_bytes, prefix_length);
        }
    }

    public IPv4Address HostId
    {
        get
        {
            var host_bytes    = new byte[4];        // Initialize a new byte array for the host ID.
            var ip_bytes      = _bytes;             // Byte representation of this IP.
            var mask_bytes    = _mask.Bytes;        // Byte representation of subnet mask.
            var prefix_length = _mask.PrefixLength; // Prefix length equivalent of the mask.

            // filter out host bits.
            for(int i = 0; i < 4; i++)
                host_bytes[i] = (byte)(ip_bytes[i] & ~mask_bytes[i]);

            // return host ID.
            return new IPv4Address(host_bytes, prefix_length);
        }
    }

    internal uint NumericValue => GetValue(_bytes);

    // Primary constructor.
    internal IPv4Address(byte[] octets, int prefixLength)
    {
        Debug.Assert(octets.Length == 4);
        Debug.Assert(prefixLength >= 0 && prefixLength <= 32);

        // Initialize the bytes array.
        _bytes = new byte[4];
        for (int i = 0; i < 4; i++) _bytes[i] = octets[i];

        // Initialize the subnet mask.
        _mask = new IPv4SubnetMask(prefixLength);
    }

    /// <summary>
    /// Prints out IP Address, subnet mask, network ID and host ID in dot-decimal format.
    /// </summary>
    public void PrintDetails()
    {
        var ip   = _bytes;
        var mask = _mask.Bytes;
        var nid  = NetworkId.Bytes;
        var hid  = HostId.Bytes;

        string line1 = string.Format("IP Address  : {0,3}.{1,3}.{2,3}.{3,3}", ip[0], ip[1], ip[2], ip[3]);
        string line2 = string.Format("Subnet Mask : {0,3}.{1,3}.{2,3}.{3,3}", mask[0], mask[1], mask[2], mask[3]);
        string line3 = string.Format("Network ID  : {0,3}.{1,3}.{2,3}.{3,3}", nid[0], nid[1], nid[2], nid[3]);
        string line4 = string.Format("Host ID     : {0,3}.{1,3}.{2,3}.{3,3}", hid[0], hid[1], hid[2], hid[3]);

        Console.WriteLine();
        Console.WriteLine(line1);
        Console.WriteLine(line2);
        Console.WriteLine(line3);
        Console.WriteLine(line4);
        Console.WriteLine();
    }

    /// <summary>
    /// Prints out IP Address, subnet mask, network ID and host ID in binary format.
    /// </summary>
    public void PrintBinaryDetails()
    {
        var ip   = _bytes;
        var mask = _mask.Bytes;
        var nid  = NetworkId.Bytes;
        var hid  = HostId.Bytes;

        var line1 = string.Format(
            "IP Address  : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}",   ip[0],   ip[1],   ip[2],   ip[3]);

        var line2 = string.Format(
            "Subnet Mask : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}", mask[0], mask[1], mask[2], mask[3]);

        var line3 = string.Format(
            "Network ID  : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}",  nid[0],  nid[1],  nid[2],  nid[3]);

        var line4 = string.Format(
            "Host ID     : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}",  hid[0],  hid[1],  hid[2],  hid[3]);

        Console.WriteLine();
        Console.WriteLine(line1);
        Console.WriteLine(line2);
        Console.WriteLine(line3);
        Console.WriteLine(line4);
        Console.WriteLine();
    }

    /// <summary>
    /// Prints out IP Address, subnet mask, network ID and host ID in binary format.
    /// Network bits and host bits are color differently for emphasis.
    /// </summary>
    public void PrintEmphasizedBinaryDetails()
    {
        var ip_bytes   = _bytes;
        var mask_bytes = _mask.Bytes;
        var nid_bytes  = NetworkId.Bytes;
        var hid_bytes  = HostId.Bytes;

        var prefix_length = _mask.PrefixLength;
        var separator     = @".";

        var ip_emphasized   = Emphasize(ip_bytes, prefix_length, separator, false, false);
        var mask_emphasized = Emphasize(mask_bytes, prefix_length, separator, false, false);
        var nid_emphasized  = Emphasize(nid_bytes, prefix_length, separator, false, true);
        var hid_emphasized  = Emphasize(hid_bytes, prefix_length, separator, true, false);

        var line1 = $"IP Address  : {ip_emphasized}";
        var line2 = $"Subnet Mask : {mask_emphasized}";
        var line3 = $"Network ID  : {nid_emphasized}";
        var line4 = $"Host ID     : {hid_emphasized}";

        Console.WriteLine();
        Console.WriteLine(line1);
        Console.WriteLine(line2);
        Console.WriteLine(line3);
        Console.WriteLine(line4);
        Console.WriteLine();
    }

    /// <summary>
    /// Prints out IP Address, subnet mask, network ID and host ID in both dot-decimal and binary format.
    /// Network bits and host bits are colored differently for emphasis.
    /// </summary>
    public void PrintFullDetails()
    {
        var ip_bytes   = _bytes;
        var mask_bytes = _mask.Bytes;
        var nid_bytes  = NetworkId.Bytes;
        var hid_bytes  = HostId.Bytes;

        var prefix_length = _mask.PrefixLength;
        var separator     = @".";

        var ip_emphasized   = Emphasize(ip_bytes, prefix_length, separator, false, false);
        var mask_emphasized = Emphasize(mask_bytes, prefix_length, separator, false, false);
        var nid_emphasized  = Emphasize(nid_bytes, prefix_length, separator, false, true);
        var hid_emphasized  = Emphasize(hid_bytes, prefix_length, separator, true, false);

        var ip   = _bytes;
        var mask = _mask.Bytes;
        var nid  = NetworkId.Bytes;
        var hid  = HostId.Bytes;

        var ip_ddn   = string.Format("IP Address  : {0,3}.{1,3}.{2,3}.{3,3}",   ip[0],   ip[1],   ip[2],   ip[3]);
        var mask_ddn = string.Format("Subnet Mask : {0,3}.{1,3}.{2,3}.{3,3}", mask[0], mask[1], mask[2], mask[3]);
        var nid_ddn  = string.Format("Network ID  : {0,3}.{1,3}.{2,3}.{3,3}",  nid[0],  nid[1],  nid[2],  nid[3]);
        var hid_ddn  = string.Format("Host ID     : {0,3}.{1,3}.{2,3}.{3,3}",  hid[0],  hid[1],  hid[2],  hid[3]);

        var line1 = ip_ddn   + "  [ " + ip_emphasized   + " ]";
        var line2 = mask_ddn + "  [ " + mask_emphasized + " ]";
        var line3 = nid_ddn  + "  [ " + nid_emphasized  + " ]";
        var line4 = hid_ddn  + "  [ " + hid_emphasized  + " ]";

        Console.WriteLine();
        Console.WriteLine(line1);
        Console.WriteLine(line2);
        Console.WriteLine(line3);
        Console.WriteLine(line4);
        Console.WriteLine();
    }

    public IPv4Subnet[] ListMyAncestors()
    {
        var subnets = new IPv4Subnet[33];

        for (int i = 0; i < 33; i++)
            subnets[i] = new IPv4Subnet(_bytes, 32 - i);

        return subnets;
    }

    public IPv4Address Clone() => new IPv4Address(_bytes, _mask.PrefixLength);

    public override string ToString() => ToString(_settings.DisplayStyle);

    public string ToString(IPv4AddressDisplay ipFormat)
    {
        var format_string = GetFormatString(ipFormat);

        return ToString(format_string, CultureInfo.InvariantCulture);
    }

    public string ToString(string format) => ToString(format, CultureInfo.InvariantCulture);

    public string ToString(string format, IFormatProvider provider)
    {
        if (string.IsNullOrEmpty(format)) format = GetFormatString(_settings.DisplayStyle);

        switch (format.ToUpperInvariant())
        {
            case "I":
                return string.Format("{0}.{1}.{2}.{3}",
                    _bytes[0], _bytes[1], _bytes[2], _bytes[3]);

            case "P":
                return string.Format("{0}.{1}.{2}.{3}/{4}",
                    _bytes[0], _bytes[1], _bytes[2], _bytes[3], _mask.PrefixLength);

            case "M":
                var mask_bytes = _mask.Bytes;
                return string.Format("{0}.{1}.{2}.{3}/{4}.{5}.{6}.{7}",
                    _bytes[0], _bytes[1], _bytes[2], _bytes[3],
                    mask_bytes[0], mask_bytes[1], mask_bytes[2], mask_bytes[3]);

            default:
                throw new FormatException($"The \"{format}\" string is not supported.");
        }
    }

    /// <summary>
    /// Returns the IP address in the form of a binary string.
    /// </summary>
    /// <param name="separator">Separator for each byte of data</param>
    /// <returns></returns>
    public string ToBinary(string separator = " ")
    {
        string[] binary = _bytes.Select(octet => octet.ToString("b8")).ToArray();
        return string.Join(separator, binary);
    }

    /// <summary>
    /// Returns the IP address in the form of a binary string.
    /// Network bits and host bits are colored differently for emphasis.
    /// </summary>
    /// <param name="separator">Separator for each byte of data</param>
    /// <returns></returns>
    public string ToEmphasizedBinary(string separator = " ")
    {
        byte[] bytes            = _bytes;
        int    prefix_length    = _mask.PrefixLength;
        bool   dim_network_bits = false;
        bool   dim_host_bits    = false;

        return Emphasize(bytes, prefix_length, separator, dim_network_bits, dim_host_bits);
    }

    public int CompareTo(object obj)
    {
        if (obj is null) return 1; // Follow convention: non-null > null

        if (obj is not IPv4Address other)
            throw new ArgumentException($"Object is not {typeof(IPv4Address)}");
        else
            return CompareTo(other);
    }

    public int CompareTo(IPv4Address other)
    {
        if (other is null) return 1; // Follow convention: non-null > null

        byte[] this_bytes  = _bytes;
        byte[] other_bytes = other.Bytes;

        for(int i = 0 ; i < 4 ; i++)
        {
            if (this_bytes[i] > other_bytes[i]) return 1;
            if (this_bytes[i] < other_bytes[i]) return -1;
        }
        return 0;
    }

    public bool Equals(IPv4Address other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        return CompareTo(other) == 0;
    }

    public override bool Equals(object obj) => Equals(obj as IPv4Address);

    public override int GetHashCode()
    {
        int hash = 13;

        for(int i = 0; i < _bytes.Length; i++)
            hash = hash * 17 + _bytes[i].GetHashCode();

        hash = hash * 17 + _mask.PrefixLength.GetHashCode();

        return hash;
    }

    public static IPv4Address Parse(string ip)
    {
        // try to match cidr pattern
        if (Regex.IsMatch(ip, IPPattern.IPv4Subnet))
        {
            var parts  = ip.Split('/');
            var prefix = parts[0];
            var suffix = Convert.ToByte(parts[1]);
            var octets = prefix.Split('.').Select(octet => Convert.ToByte(octet)).ToArray();

            return new IPv4Address(octets, suffix);
        }

        // try to match ip pattern
        if (Regex.IsMatch(ip, IPPattern.IPv4Address))
        {
            var octets = ip.Split('.').Select(octet => Convert.ToByte(octet)).ToArray();
            return new IPv4Address(octets, 32);
        }

        throw new ArgumentException("IP is not valid");
    }

    // Produce a random IPv4Address object.
    public static IPv4Address GetRandom()
    {
        var octets        = new byte[4];
        var rand          = new Random();
        var prefix_length = 32;

        rand.NextBytes(octets);

        return new IPv4Address(octets, prefix_length);
    }

    // Helper function to ANSI-code the network bits and host bits in different colors.
    protected static string Emphasize(
        byte[] bytes, int prefixLength, string separator, bool dimNetworkBits, bool dimHostBits)
    {
        // Save a binary string representation of the IP.
        string[] bytes_in_binary = bytes.Select(octet => octet.ToString("b8")).ToArray();
        string   binary          = string.Join(string.Empty, bytes_in_binary);

        // Get the network bits and host bits.
        string network_bits = binary.Substring(0, prefixLength);
        string host_bits    = binary.Remove(0, prefixLength);

        // save the index in the host bits string to insert the first separator to host bits.
        // e.g. prefix length   = 18
        //      mask            = 11111111_11111111_11000000_00000000 (underscore is for visual aid)
        //      host bits       = 000000_00000000
        //      separator index = 6
        int first_host_separator_index = 8 - (prefixLength % 8);

        // insert separators to the network bits
        for(int i = 8 ; i <= network_bits.Length && i < 32 ; i += 8+separator.Length)
            network_bits = network_bits.Insert(i, separator);

        // insert separators to the host bits
        for(int j = first_host_separator_index ; j < host_bits.Length ; j += 8+separator.Length)
            host_bits = host_bits.Insert(j, separator);


        string format_reset = Style.Instance.Reset;

        string format_network_bits = dimNetworkBits
            ? _settings.NetworkBitsStyle + Style.Instance.Dim
            : _settings.NetworkBitsStyle;

        string format_host_bits = dimHostBits
            ? _settings.HostBitsStyle + Style.Instance.Dim
            : _settings.HostBitsStyle;

        // return emphasized string
        return $"{format_network_bits}{network_bits}{format_reset}{format_host_bits}{host_bits}{format_reset}";
    }

    // Helper function to map format enum to format string.
    protected static string GetFormatString(IPv4AddressDisplay ipFormat)
    {
        switch (ipFormat)
        {
            case IPv4AddressDisplay.IP:              return "I";
            case IPv4AddressDisplay.IP_PrefixLength: return "P";
            case IPv4AddressDisplay.IP_Mask:         return "M";
            default:                                 return "I";
        }
    }

    // Helper function to get the numerical value of an IPv4 address from it's byte array.
    protected static uint GetValue(byte[] bytes)
    {
        return ((uint)bytes[0] << 24) +
                ((uint)bytes[1] << 16) +
                ((uint)bytes[2] <<  8) +
                ((uint)bytes[3] <<  0);
    }
}