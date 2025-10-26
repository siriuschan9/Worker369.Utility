using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Worker369.Utility;

public class IPv6Address :IComparable, IComparable<IPv6Address>, IEquatable<IPv6Address>, IFormattable
{
    protected static readonly IPv6AddressSettings _settings =
        IPv6AddressSettings.Instance;

    protected readonly byte[]         _bytes;                   // 16 byte representation (Big Endian order)
    protected readonly IPv6PrefixMask _mask;

    public byte[] Bytes => (byte[])_bytes.Clone();              // Returns a copy of the bytes.

    public string IPAddress => ToString(IPv6AddressDisplay.IP); // By default, print out IP portion only.

    public IPv6PrefixMask PrefixMask => _mask;

    public int PrefixLength
    {
        get { return _mask.PrefixLength; }
        set { _mask.PrefixLength = value; }
    }

    public IPv6Address NetworkId
    {
        get
        {
            var network_bytes = new byte[16];       // Initialize a new byte array for the network ID.
            var ip_bytes      = _bytes;             // Byte representation of this IP.
            var mask_bytes    = _mask.Bytes;        // Byte representation of subnet mask.
            var prefix_length = _mask.PrefixLength; // Prefix length equivalent of the mask.

            // filter out network bits.
            for(int i = 0; i < 16; i++)
                network_bytes[i] = (byte)(ip_bytes[i] & mask_bytes[i]);

            // return network ID.
            return new IPv6Address(network_bytes, prefix_length);
        }
    }

    public IPv6Address HostId
    {
        get
        {
            var host_bytes    = new byte[16];         // Initialize a new byte array for the host ID.
            var ip_bytes      = _bytes;               // Byte representation of this IP.
            var mask_bytes    = _mask.Bytes;          // Byte representation of subnet mask.
            var prefix_length = _mask.PrefixLength;   // Prefix length equivalent of the mask.

            // filter out host bits.
            for(int i = 0; i < 16; i++)
                host_bytes[i] = (byte)(ip_bytes[i] & ~mask_bytes[i]);

            // return host ID.
            return new IPv6Address(host_bytes, prefix_length);
        }
    }

    internal BigInteger NumericValue => GetValue(_bytes);

    // Primary constructor.
    internal IPv6Address(byte[] octets, int prefixLength)
    {
        Debug.Assert(octets.Length == 16);
        Debug.Assert(prefixLength >= 0 && prefixLength <= 128);

        // Initialize the bytes array.
        _bytes = new byte[16];
        for (int i = 0; i < 16; i++) _bytes[i] = octets[i];

        // Initialize the subnet mask.
        _mask = new IPv6PrefixMask(prefixLength);
    }

    /// <summary>
    /// Prints out IP Address, subnet mask, network ID and host ID in hex-colon format.
    /// </summary>
    public void PrintDetails()
    {
        var ip     = _bytes;
        var mask = _mask.Bytes;
        var nid    = NetworkId.Bytes;
        var hid    = HostId.Bytes;

        string line1 = string.Format(
            "IP Address  : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            ip[0], ip[1],  ip[2], ip[3],  ip[4],  ip[5],  ip[6],  ip[7],
            ip[8], ip[9], ip[10], ip[11], ip[12], ip[13], ip[14], ip[15]
        );

        string line2 = string.Format(
            "Prefix Mask : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            mask[0], mask[1], mask[2],  mask[3],  mask[4],  mask[5],  mask[6],  mask[7],
            mask[8], mask[9], mask[10], mask[11], mask[12], mask[13], mask[14], mask[15]
        );

        string line3 = string.Format(
            "Network ID  : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            nid[0], nid[1], nid[2],  nid[3],  nid[4],  nid[5],  nid[6],  nid[7],
            nid[8], nid[9], nid[10], nid[11], nid[12], nid[13], nid[14], nid[15]
        );

        string line4 = string.Format(
            "Host ID     : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            hid[0], hid[1], hid[2],  hid[3],  hid[4],  hid[5],  hid[6],  hid[7],
            hid[8], hid[9], hid[10], hid[11], hid[12], hid[13], hid[14], hid[15]
        );

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
        var ip     = _bytes;
        var mask = _mask.Bytes;
        var nid    = NetworkId.Bytes;
        var hid    = HostId.Bytes;

        string line1 = string.Format(
            "IP Address  : {0:b8}{1:b8}:{2:b8}{3:b8}:{4:b8}{5:b8}:{6:b8}{7:b8}:" +
            "{8:b8}{9:b8}:{10:b8}{11:b8}:{12:b8}{13:b8}:{14:b8}{15:b8}",
            ip[0], ip[1], ip[2],  ip[3],  ip[4],  ip[5],  ip[6],  ip[7],
            ip[8], ip[9], ip[10], ip[11], ip[12], ip[13], ip[14], ip[15]
        );

        string line2 = string.Format(
            "Prefix Mask : {0:b8}{1:b8}:{2:b8}{3:b8}:{4:b8}{5:b8}:{6:b8}{7:b8}:" +
            "{8:b8}{9:b8}:{10:b8}{11:b8}:{12:b8}{13:b8}:{14:b8}{15:b8}",
            mask[0], mask[1], mask[2],  mask[3],  mask[4],  mask[5],  mask[6],  mask[7],
            mask[8], mask[9], mask[10], mask[11], mask[12], mask[13], mask[14], mask[15]
        );

        string line3 = string.Format(
            "Network ID  : {0:b8}{1:b8}:{2:b8}{3:b8}:{4:b8}{5:b8}:{6:b8}{7:b8}:" +
            "{8:b8}{9:b8}:{10:b8}{11:b8}:{12:b8}{13:b8}:{14:b8}{15:b8}",
            nid[0], nid[1], nid[2],  nid[3],  nid[4],  nid[5],  nid[6],  nid[7],
            nid[8], nid[9], nid[10], nid[11], nid[12], nid[13], nid[14], nid[15]
        );

        string line4 = string.Format(
            "Host ID     : {0:b8}{1:b8}:{2:b8}{3:b8}:{4:b8}{5:b8}:{6:b8}{7:b8}:" +
            "{8:b8}{9:b8}:{10:b8}{11:b8}:{12:b8}{13:b8}:{14:b8}{15:b8}",
            hid[0], hid[1], hid[2],  hid[3],  hid[4],  hid[5],  hid[6],  hid[7],
            hid[8], hid[9], hid[10], hid[11], hid[12], hid[13], hid[14], hid[15]
        );

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

        var ip_emphasized     = Emphasize(ip_bytes, prefix_length, separator, false, false);
        var prefix_emphasized = Emphasize(mask_bytes, prefix_length, separator, false, false);
        var nid_emphasized    = Emphasize(nid_bytes, prefix_length, separator, false, true);
        var hid_emphasized    = Emphasize(hid_bytes, prefix_length, separator, true, false);

        var line1 = $"IP Address  : {ip_emphasized}";
        var line2 = $"Prefix Mask : {prefix_emphasized}";
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
    /// Prints out IP Address, subnet mask, network ID and host ID in both hex-colon and binary format.
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

        var ip_emphasized     = Emphasize(ip_bytes, prefix_length, separator, false, false);
        var prefix_emphasized = Emphasize(mask_bytes, prefix_length, separator, false, false);
        var nid_emphasized    = Emphasize(nid_bytes, prefix_length, separator, false, true);
        var hid_emphasized    = Emphasize(hid_bytes, prefix_length, separator, true, false);

        var ip   = _bytes;
        var mask = _mask.Bytes;
        var nid  = NetworkId.Bytes;
        var hid  = HostId.Bytes;

        // hex-colon notation for ip
        var ip_hcn   = string.Format(
            "IP Address  : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            ip[0], ip[1], ip[2],  ip[3],  ip[4],  ip[5],  ip[6],  ip[7],
            ip[8], ip[9], ip[10], ip[11], ip[12], ip[13], ip[14], ip[15]
        );

        // hex-colon notation for mask
        var mask_hcn = string.Format(
            "Prefix Mask : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            mask[0], mask[1], mask[2],  mask[3],  mask[4],  mask[5],  mask[6],  mask[7],
            mask[8], mask[9], mask[10], mask[11], mask[12], mask[13], mask[14], mask[15]
        );

        // hex-colon notation for network id
        var nid_hcn  = string.Format(
            "Network ID  : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            nid[0], nid[1], nid[2], nid[3], nid[4], nid[5], nid[6], nid[7],
            nid[8], nid[9], nid[10], nid[11], nid[12], nid[13], nid[14], nid[15]
        );

        // hex-colon notation for host id
        var hid_hcn  = string.Format(
            "Host ID     : {0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:" +
            "{8:x2}{9:x2}:{10:x2}{11:x2}:{12:x2}{13:x2}:{14:x2}{15:x2}",
            hid[0], hid[1], hid[2],  hid[3],  hid[4],  hid[5],  hid[6],  hid[7],
            hid[8], hid[9], hid[10], hid[11], hid[12], hid[13], hid[14], hid[15]
        );

        var line1 = ip_hcn   + "  [ " + ip_emphasized   + " ]";
        var line2 = mask_hcn + "  [ " + prefix_emphasized + " ]";
        var line3 = nid_hcn  + "  [ " + nid_emphasized  + " ]";
        var line4 = hid_hcn  + "  [ " + hid_emphasized  + " ]";

        Console.WriteLine();
        Console.WriteLine(line1);
        Console.WriteLine(line2);
        Console.WriteLine(line3);
        Console.WriteLine(line4);
        Console.WriteLine();
    }

    public IPv6Address Clone() => new IPv6Address(_bytes, _mask.PrefixLength);

    public override string ToString() => ToString(_settings.DisplayStyle);

    public string ToString(IPv6AddressDisplay ipFormat)
    {
        var format_string = GetFormatString(ipFormat);

        return ToString(format_string, CultureInfo.InvariantCulture);
    }

    public string ToString(string format) => ToString(format, CultureInfo.InvariantCulture);

    public string ToString(string format, IFormatProvider provider)
    {
        if (string.IsNullOrEmpty(format)) format = GetFormatString(_settings.DisplayStyle);

        // Delegate compression to System.Net.IPAddress.
        string compressed = new IPAddress(_bytes).ToString();

        switch (format.ToUpperInvariant())
        {
            case "I": return compressed;
            case "P": return compressed + "/" + _mask.PrefixLength;
            default:  throw new FormatException($"The \"{format}\" string is not supported.");
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

        if (obj is not IPv6Address other)
            throw new ArgumentException($"Object is not {typeof(IPv6Address)}");
        else
            return CompareTo(other);
    }

    public int CompareTo(IPv6Address other)
    {
        if (other is null) return 1; // Follow convention: non-null > null

        byte[] this_bytes  = _bytes;
        byte[] other_bytes = other.Bytes;

        for(int i = 0 ; i < 16 ; i++)
        {
            if (this_bytes[i] > other_bytes[i]) return 1;
            if (this_bytes[i] < other_bytes[i]) return -1;
        }
        return 0;
    }

    public bool Equals(IPv6Address other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        return CompareTo(other) == 0;
    }

    public override bool Equals(object obj) => Equals(obj as IPv6Address);

    public override int GetHashCode()
    {
        int hash = 17;

        for(int i = 0; i < _bytes.Length; i++)
            hash = hash * 19 + _bytes[i].GetHashCode();

        hash = hash * 19 + _mask.PrefixLength.GetHashCode();

        return hash;
    }

    public static IPv6Address Parse(string ip)
    {
        // try to match cidr pattern first
        // a valid cidr pattern will also match ip pattern
        // if we match ip pattern first, IPAddress.Parse will throw "Invalid IP Address" error.

        // try to match cidr pattern
        if (Regex.IsMatch(ip, IPPattern.IPv6Subnet))
        {
            var parts         = ip.Split('/');
            var prefix        = parts[0];
            var prefix_length = Convert.ToByte(parts[1]);

            var ip_address = System.Net.IPAddress.Parse(prefix);
            var bytes      = ip_address.GetAddressBytes();

            return new IPv6Address(bytes, prefix_length);
        }

        // try to match ip pattern
        if (Regex.IsMatch(ip, IPPattern.IPv6Address))
        {
            var ip_address  = System.Net.IPAddress.Parse(ip);
            var bytes       = ip_address.GetAddressBytes();

            return new IPv6Address(bytes, 128);
        }


        throw new ArgumentException("IP is not valid");
    }

    // Produce a random IPv6Address object.
    public static IPv6Address GetRandom()
    {
        var octets        = new byte[16];
        var rand          = new Random();
        var prefix_length = 128;

        rand.NextBytes(octets);

        return new IPv6Address(octets, prefix_length);
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
        for(int i = 8 ; i <= network_bits.Length && i < 128 ; i += 8+separator.Length)
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
        return $"{format_network_bits}{network_bits}{format_host_bits}{host_bits}{format_reset}";
    }

    // Helper function to map format enum to format string.
    protected static string GetFormatString(IPv6AddressDisplay ipFormat)
    {
        switch (ipFormat)
        {
            case IPv6AddressDisplay.IP:              return "I";
            case IPv6AddressDisplay.IP_PrefixLength: return "P";
            default:                                 return "I";
        }
    }

    // Helper function to get the numerical value of an IPv6 address from it's byte array.
    protected static BigInteger GetValue(byte[] bytes)
    {
        var value = BigInteger.Zero;                            // Initialize value to 0 first.

        for(int i = 0, j = 120; i <= bytes.Length; i++, j -= 8) // bytes[0]<<120 + bytes[1]<<112 +  ... bytes[15]<<0
            value += (BigInteger)bytes[i] << j;

        return value;
    }
}