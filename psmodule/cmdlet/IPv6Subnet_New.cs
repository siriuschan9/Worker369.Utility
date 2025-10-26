using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "IPv6Subnet")]
[Alias("subnet6")]
[OutputType(typeof(IPv6Subnet))]
public class IPv6Subnet_New : PSCmdlet
{
    [Parameter(Position = 0, ValueFromPipeline = true)]
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

    protected override void ProcessRecord()
    {
        if (string.IsNullOrEmpty(Cidr))
            WriteObject(IPv6Subnet.GetRandom());
        else
            WriteObject(IPv6Subnet.Parse(Cidr));
    }
}