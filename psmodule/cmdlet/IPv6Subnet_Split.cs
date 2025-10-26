using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Split, "IPv6Subnet", DefaultParameterSetName = "String")]
[Alias("split6")]
[OutputType(typeof(IPv6Subnet[]))]
public class IPv6Subnet_Split : PSCmdlet
{
    [Parameter(ParameterSetName = "IPv6Subnet", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public IPv6Subnet Subnet { get; set; }

    [Parameter(ParameterSetName = "String", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    [ValidatePattern(
        @"^(" +                                                // Alternation - Start
        @"([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|" +           // 1:2:3:4:5:6:7:8
        @"([0-9a-fA-F]{1,4}:){1,7}:|" +                        // 1::, 1:2::, .., 1:2:3:4:5:6:7::
        @"([0-9a-fA-F]{1,4}:){1,6}(:[0-9a-fA-F]{1,4}){1,1}|" + // 1::2, 1:2::3, .., 1:2:3:4:5:6::7
        @"([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|" + // 1::2:3, 1:2::3:4, ..., 1:2:3:4:5::6:7
        @"([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|" + // 1::2:3:4, 1:2::2:3:4, ..., 1:2:3:4::5:6:7
        @"([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|" + // 1::2:3:4:5, 1:2::3:4:5:6, .., 1:2:3::4:5:6:7
        @"([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|" + // 1::2:3:4:5:6, 1:2::3:4:5:6:7
        @"([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}|" + // 1::2:3:4:5:6:7
        @":(:[0-9a-fA-F]{1,4}){1,7}|" +                        // ::, ::1, ::1:2, .., ::1:2:3:4:5:6:7
        @"::" +                                                // ::
        @")" +                                                 // Alternation - End
        @"\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8])$"           // /128
    )]
    public string Cidr { get; set; }

    [Parameter(Position = 1)]
    [ValidateRange(0, 129)]
    public int Bits { get; set; } = 1;

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "IPv6Subnet")
        {
            WriteObject(IPv6Subnet.Split(Subnet, Bits), true);
            return;
        }

        if (ParameterSetName == "String")
        {
            WriteObject(IPv6Subnet.Split(IPv6Subnet.Parse(Cidr), Bits), true);
            return;
        }
    }
}