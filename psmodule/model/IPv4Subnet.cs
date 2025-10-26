using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Worker369.Utility;

public class IPv4Subnet : IComparable, IComparable<IPv4Subnet>, IEquatable<IPv4Subnet>
{
    // The network address defines this subnet.
    protected IPv4Address _network_address;

    public string CIDR => _network_address.ToString(IPv4AddressDisplay.IP_PrefixLength);

    public IPv4Address FirstIP => _network_address.Clone();

    public IPv4Address LastIP
    {
        get
        {
            byte[] my_bytes     = _network_address.Bytes;
            byte[] subnet_mask  = _network_address.SubnetMask.Bytes;
            byte[] inverse_mask = subnet_mask.Select(b => (byte)~b).ToArray();

            byte[] new_bytes = new byte[4];
            for (int i = 0; i < 4; i++)
                new_bytes[i] = (byte)(my_bytes[i] | inverse_mask[i]);

            return new IPv4Address(new_bytes, _network_address.PrefixLength);
        }
    }

    public IPv4SubnetMask SubnetMask => _network_address.SubnetMask;

    public NumberInfo Size => _network_address.SubnetMask.Size;

    public int PrefixLength
    {
        get { return _network_address.PrefixLength;}
        set
        {
            _network_address.PrefixLength = value;         // Change the prefix length first.
            _network_address = _network_address.NetworkId; // Normalize the CIDR.
        }
    }

    internal IPv4Subnet(byte[] octets, int prefixLength)
    {
        _network_address = new IPv4Address(octets, prefixLength).NetworkId;
    }

    /// <summary>
    /// Returns an IEnumerable of IP addresses in string format.
    /// </summary>
    public IEnumerable<string> ListIPAddresses()
    {
        BigInteger subnet_size  = _network_address.SubnetMask.Size.Value;
        BigInteger subnet_value = _network_address.NumericValue;

        for (BigInteger i = BigInteger.Zero; i < subnet_size; i++)
        {
            BigInteger ip_value = subnet_value + i;

            byte octet1 = (byte)((ip_value & 0xff000000) >> 24);  // [ X ] . [   ] . [   ] . [   ]
            byte octet2 = (byte)((ip_value & 0x00ff0000) >> 16);  // [   ] . [ X ] . [   ] . [   ]
            byte octet3 = (byte)((ip_value & 0x0000ff00) >> 8);   // [   ] . [   ] . [ X ] . [   ]
            byte octet4 = (byte)((ip_value & 0x000000ff) >> 0);   // [   ] . [   ] . [   ] . [ X ]

            yield return string.Format("{0}.{1}.{2}.{3}", octet1, octet2, octet3, octet4);
        }
    }

    /// <summary>
    /// Returns an IEnumerable of IP addresses in IPv4Adress type.
    /// </summary>
    public IEnumerable<IPv4Address> GetIPAddresses()
    {
        BigInteger        subnet_size   = _network_address.SubnetMask.Size.Value;
        BigInteger        subnet_value  = _network_address.NumericValue;
        int               prefix_length = _network_address.PrefixLength;

        for (BigInteger i = BigInteger.Zero; i < subnet_size; i++)
        {
            BigInteger ip_value = subnet_value + i;

            byte octet1 = (byte)((ip_value & 0xff000000) >> 24); // [ X ] . [   ] . [   ] . [   ]
            byte octet2 = (byte)((ip_value & 0x00ff0000) >> 16); // [   ] . [ X ] . [   ] . [   ]
            byte octet3 = (byte)((ip_value & 0x0000ff00) >> 8);  // [   ] . [   ] . [ X ] . [   ]
            byte octet4 = (byte)((ip_value & 0x000000ff) >> 0);  // [   ] . [   ] . [   ] . [ X ]

            yield return new IPv4Address(new byte[]{octet1, octet2, octet3, octet4}, prefix_length);
        }
    }

    public bool ContainsIpAddress(IPv4Address ip)
    {
        var clone = ip.Clone();
        clone.PrefixLength = _network_address.PrefixLength;

        return _network_address.Equals(clone.NetworkId);
    }

    public bool ContainsIpAddress(string ip) => ContainsIpAddress(IPv4Address.Parse(ip));

    /// <summary>
    /// Returns the next adjacent subnet.
    /// </summary>
    /// <param name="n">The number subnets to return.</param>
    public IEnumerable<IPv4Subnet> Next(int n = 1)
    {
        if (n < 1)
            throw new ArgumentException("The parameter must be greater than 0", nameof(n));

        var prefix_length   = _network_address.PrefixLength;            // Save a copy of the prefix length.
        var bytes           = _network_address.Bytes;                   // Grab a copy of bytes data.
        var increment_index = (prefix_length - 1) / 8;                  // byte index to add the increment.
        var increment_value = (byte)(1 << ((32 - prefix_length) % 8));  // increment size within the byte.

        int returned = 0;                                               // Track number of returned subnets.
        while (returned < n)
        {
            if (bytes[increment_index] + increment_value == 256)        // Check if the increment needs a carry.
            {
                bytes[increment_index] = 0;                             // If carry is need, zero this byte.

                int carry_index = increment_index - 1;                  // Start the carry from the "next" byte.
                while (carry_index >= 0)                                // Carry until the MSB is reached.
                {
                    if (bytes[carry_index] + 1 == 256)                  // Check if this carry needs another carry.
                    {
                        bytes[carry_index] = 0;                         // If carry is needed, zero current byte.
                        carry_index--;
                    }
                    else
                    {
                        bytes[carry_index] += 1;                        // No further carry is needed. Break here.
                        break;
                    }
                }

                if (bytes.All(octet => octet == 0)) break;              // No more available address space.
            }
            else
                bytes[increment_index] += increment_value;              // No carry needed.

            // Yield one subnet with current bytes data before moving on to next increment.
            yield return new IPv4Subnet(bytes, prefix_length); returned++;
        }
    }

    /// <summary>
    /// Returns the previous adjacent subnet.
    /// </summary>
    /// <param name="n">The number subnets to return.</param>
    public IEnumerable<IPv4Subnet> Previous(int n = 1)
    {
        if (n < 1)
            throw new ArgumentException("The parameter must be greater than 0", nameof(n));

        var prefix_length   = _network_address.PrefixLength;            // Save a copy of the prefix length.
        var bytes           = _network_address.Bytes;                   // Grab a copy of bytes data.
        var decrement_index = (prefix_length - 1) / 8;                  // byte index to substract the decrement.
        var decrement_value = (byte)(1 << ((32 - prefix_length) % 8));  // decrement size within the byte.

        int returned = 0;                                               // Track number of returned subnets.
        while (returned < n)
        {
            if (bytes[decrement_index] == 0)                            // Check if the decrement needs a borrow.
            {
                bytes[decrement_index] = (byte)(256 - decrement_value); // If borrow is need, add 256 to this byte.

                int borrow_index = decrement_index - 1;                 // Borrow from the "next" byte.
                while (borrow_index >= 0)                               // Borrow until the MSB is reached.
                {
                    if (bytes[borrow_index] == 0)                       // Check if this byte needs another borrow.
                    {
                        bytes[borrow_index] = 255;                      // If borrow is needed, 255 current byte.
                        borrow_index--;
                    }
                    else
                    {
                        bytes[borrow_index] -= 1;                       // No further borrow is needed. Break here.
                        break;
                    }
                }
            }
            else
                bytes[decrement_index] -= decrement_value;              // No borrow needed.

            // Yield one subnet with current bytes data before moving on to next increment.
            yield return new IPv4Subnet(bytes, prefix_length); returned++;

            if (bytes.All(octet => octet == 0)) break;                  // No more available address space.
        }
    }

    public override string ToString() =>_network_address.ToString(IPv4AddressDisplay.IP_PrefixLength);

    public int CompareTo(object obj)
    {
        if (obj is null) return 1; // Follow convention: non-null > null

        if (obj is not IPv4Subnet other)
            throw new ArgumentException($"Object is not {typeof(IPv4Subnet)}");
        else
            return CompareTo(other);
    }

    public int CompareTo(IPv4Subnet other)
    {
        if (other is null) return 1; // Follow convention: non-null > null

        byte[] this_bytes  = _network_address.Bytes;
        byte[] other_bytes = other._network_address.Bytes;

        for(int i = 0 ; i < 4 ; i++)
        {
            if (this_bytes[i] > other_bytes[i]) return 1;
            if (this_bytes[i] < other_bytes[i]) return -1;
        }

        int this_prefix_length  = _network_address.PrefixLength;
        int other_prefix_length = other._network_address.PrefixLength;

        return this_prefix_length.CompareTo(other_prefix_length);
    }

    public bool Equals(IPv4Subnet other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        return CompareTo(other) == 0;
    }

    public override bool Equals(object obj) => Equals(obj as IPv4Subnet);

    public override int GetHashCode() => _network_address.GetHashCode();

    public static bool IsOverlapping(IPv4Subnet a, IPv4Subnet b)
    {
        // We want to know if the smaller subnet (higher prefix) is inside the larger subnet (smaller prefix).
        int compare_prefix = Math.Min(a.PrefixLength, b.PrefixLength);

        byte[] a_bytes = a._network_address.Bytes;
        byte[] b_bytes = b._network_address.Bytes;

        for (int i = 0; i < 4 && i * 8 < compare_prefix; i++)
        {
            // Initialize mask to 255 first
            byte mask = 255;

            // If the compare prefix falls on this byte, align the mask to the prefix.
            if (i * 8 == (compare_prefix / 8 * 8)) mask <<= 8 - (compare_prefix % 8);

            // If uncommon network bits are detected in this byte - return false here.
            if ((a_bytes[i] & mask) != (b_bytes[i] & mask)) return false;
        }

        // All network bits up to the larger subnet prefix length are the same - return true.
        return true;
    }

    public static IEnumerable<IPv4Subnet> Split(IPv4Subnet parent, int bits)
    {
        int parent_prefix_length = parent.PrefixLength;
        int child_prefix_length  = parent_prefix_length + bits;

        if (bits < 0 || bits > 32)    throw new ArgumentException("Bits must be between 0 and 32", nameof(bits));
        if (child_prefix_length > 32) throw new OverflowException("Network bits of child subnets exceeded 32");

        byte[] parent_bytes = parent._network_address.Bytes;

        // uint.MaxValue = 4,294,967,295 | 2^32 = 4,294,967,296 => hence, requires ulong.
        ulong count     = 1ul << bits ; // Number of child subnets to make.
        int  shift_bits = 32 - child_prefix_length; // Shift child differentiator bits align on the prefix length.

        for (ulong i = 0; i < count; i++) // Number of child subnets.
        {
            // Initialize child bytes.
            byte[] child_bytes = GetBytes((uint)i);

            // Left shift the bytes until the prefix length.
            ShiftLeft(child_bytes, shift_bits);

            // Perform bitwise OR with the parent bytes to form the bytes for this child subnet.
            for (int j = 0; j < 4; j++)
                child_bytes[j] |= parent_bytes[j];

            yield return new IPv4Subnet(child_bytes, child_prefix_length);
        }
    }

    public static IPv4Subnet Merge(IPv4Subnet[] subnets)
    {
        // Short-circuit.
        if (subnets.Length == 1) return subnets[0];

        // Network addresses of all subnets.
        var network_ids = subnets.Select(s => s._network_address.Bytes);

        // First subnet in the list.
        var first_id = network_ids.ElementAt(0);

        // Initialize mask to track common bits.
        var mask_bytes = new byte[] {0, 0, 0, 0};

        int i = 0, j = 0;

        for (i = 0; i < 4; i++)           // Byte index counter
        {
            for (j = 0; j < 8; j++)       // Bit index counter
            {
                // Walk the bits from left to right
                byte mask = (byte)(128 >> j);

                // Check if this bit is the same for all subnets.
                bool is_common_bit = network_ids.All(bytes => {
                    return (bytes[i] & mask) == (first_id[i] & mask);
                });

                if (is_common_bit)
                    mask_bytes[i] |= mask; // If this bit is a common bit, flip this bit in the mask to 1.
                else
                {
                    // Zero out all uncommon bits.
                    byte[] common_bytes = new byte[]{
                        (byte)(first_id[0] & mask_bytes[0]),
                        (byte)(first_id[1] & mask_bytes[1]),
                        (byte)(first_id[2] & mask_bytes[2]),
                        (byte)(first_id[3] & mask_bytes[3]),
                    };

                    return new IPv4Subnet(common_bytes, i * 8 + j % 8);
                }
            }
        }

        Debug.Assert(mask_bytes.All(octet => octet == 0)); // No common network bits - mask must be all 0.
        Debug.Assert(i * 8 + j % 8 == 32);                 // No common network bits - all bits must be walked.

        return new IPv4Subnet(new byte[] {0, 0, 0, 0}, 0); // No common network bits - return 0.0.0.0/0
    }

    public static IPv4Subnet Parse(string cidr)
    {
        if (TryParse(cidr, out IPv4Subnet subnet))
            return subnet;

        throw new ArgumentException("CIDR is not valid");
    }

    public static bool TryParse(string cidr, out IPv4Subnet subnet)
    {
        if (Regex.IsMatch(cidr, IPPattern.IPv4Address))
            cidr += "/32";

        if (Regex.IsMatch(cidr, IPPattern.IPv4Subnet))
        {
            string[] parts  = cidr.Split('/');
            string   prefix = parts[0];
            int      suffix = Convert.ToInt32(parts[1]);
            byte[]   octets = prefix.Split('.').Select(octet => Convert.ToByte(octet)).ToArray();

            subnet = new IPv4Subnet(octets, suffix);
            return true;
        }

        subnet = null; return false;
    }

    public static IPv4Subnet GetRandom()
    {
        var rand          = new Random();
        int prefix_length = rand.Next(33);

        uint value = 0;

        for (int i = 31; i >= 31 - prefix_length; i--) // Randomize network bits only.
            value |= (uint)(rand.Next(2) << i);

        return new IPv4Subnet(GetBytes(value), prefix_length);
    }

    public static bool operator ==(IPv4Subnet a, IPv4Subnet b) => a.Equals(b);

    public static bool operator !=(IPv4Subnet a, IPv4Subnet b) => !a.Equals(b);

    protected static byte[] GetBytes(BigInteger value)
    {
        byte[] bytes = new byte[4];

        bytes[0] = (byte)((value >> 24) & 0xff);
        bytes[1] = (byte)((value >> 16) & 0xff);
        bytes[2] = (byte)((value >> 8)  & 0xff);
        bytes[3] = (byte)((value >> 0)  & 0xff);

        return bytes;
    }

    protected static void ShiftLeft(byte[] bytes, int shiftBits)
    {
        uint carry_value = 0;

        for (int i = bytes.Length - 1; i >=0; i--)      // Walk the bytes from right to left.
        {
            var shifted_value = (uint)bytes[i] << shiftBits;
            shifted_value |= carry_value;               // Add in the carry portion from the previous byte.

            bytes[i] = (byte)(shifted_value & 0xff);    // Trim off the carry portion.

            carry_value = shifted_value >> 8;           // Extract out the carry portion.
        }
    }
}