@{
    IP = @{
        NetworkBits = "`e[95m"
        HostBits    = "`e[96m]"
    }
    Column = @{
        Header = "`e[32m]"
        Border = "`e[2m]"
        RightAlignTypes = @(
            'System.Byte',
            'System.SByte',
            'System.Int16',
            'System.Int32',
            'System.Int64',
            'System.UInt16',
            'System.UInt32',
            'System.UInt64',
            'System.Numerics.BigInteger',
            'System.Decimal',
            'System.Double',
            'System.Boolean',
            'System.DateTime',
            'Worker369.Utility.IPv4Address',
            'Worker369.Utility.IPv4Subnet',
            'Worker369.Utility.IPv6Address',
            'Worker369.Utility.IPv6Subnet'
        )
    }

}