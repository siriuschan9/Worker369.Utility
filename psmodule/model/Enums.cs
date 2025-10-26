namespace Worker369.Utility;

public enum NumberDisplay
{
    None,          // e.g. 1,500 B; 2,300,000
    HumanReadable, // e.g. 1.5k; 2.3m
    UserFriendly   // e.g. 1.5 thousand; 2.3 million
}

public enum ByteMetric
{
    SI,  // Powers of 10 (1 KB = 1,000 B)
    IEC  // Powers of 2 (1 KiB = 1,024 B)
}

public enum IPv4AddressDisplay
{
    IP,
    IP_PrefixLength,
    IP_Mask,
}

public enum IPv6AddressDisplay
{
    IP,
    IP_PrefixLength
}