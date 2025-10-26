using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsDiagnostic.Test, "IPv6CidrOverlap", DefaultParameterSetName = "String")]
[Alias("overlap6?")]
[OutputType(typeof(bool))]
public class IPv6CidrOverlap_Test : PSCmdlet
{
    [Parameter(ParameterSetName = "String", Position = 0, Mandatory = true)]
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
        @"::" +                                               // ::
        @")" +                                                 // Alternation - End
        @"\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8])$")]         // /128
    public string Cidr1 { get; set; }

    [Parameter(ParameterSetName = "String", Position = 1, Mandatory = true)]
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
        @"::" +                                               // ::
        @")" +                                                 // Alternation - End
        @"\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8])$")]         // /128
    public string Cidr2 { get; set; }

    [Parameter(ParameterSetName = "IPv6Subnet", Position = 0, Mandatory = true)]
    public IPv6Subnet Subnet1 { get; set; }

    [Parameter(ParameterSetName = "IPv6Subnet", Position = 1, Mandatory = true)]
    public IPv6Subnet Subnet2 { get; set; }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "String")
        {
            WriteObject(IPv6Subnet.IsOverlapping(IPv6Subnet.Parse(Cidr1), IPv6Subnet.Parse(Cidr2)));
            return;
        }

        if (ParameterSetName == "IPv6Subnet")
        {
            WriteObject(IPv6Subnet.IsOverlapping(Subnet1, Subnet2));
            return;
        }
    }
}