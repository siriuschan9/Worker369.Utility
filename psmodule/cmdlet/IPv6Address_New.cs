using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "IPv6Address")]
[Alias("ip6")]
[OutputType(typeof(IPv6Address))]
public class IPv6Address_New : PSCmdlet
{
    [Parameter(Position = 0)]
    [ValidatePattern(
        @"^(" +
        @"([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|" +           // 1:2:3:4:5:6:7:8
        @"([0-9a-fA-F]{1,4}:){1,7}:|" +                        // 1::, 1:2::, .., 1:2:3:4:5:6:7::
        @"([0-9a-fA-F]{1,4}:){1,6}(:[0-9a-fA-F]{1,4}){1,1}|" + // 1::2, 1:2::3, .., 1:2:3:4:5:6::7
        @"([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|" + // 1::2:3, 1:2::3:4, ..., 1:2:3:4:5::6:7
        @"([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|" + // 1::2:3:4, 1:2::2:3:4, ..., 1:2:3:4::5:6:7
        @"([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|" + // 1::2:3:4:5, 1:2::3:4:5:6, .., 1:2:3::4:5:6:7
        @"([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|" + // 1::2:3:4:5:6, 1:2::3:4:5:6:7
        @"([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}|" + // 1::2:3:4:5:6:7
        @":(:[0-9a-fA-F]{1,4}){1,7}|" +                        // ::, ::1, ::1:2, .., ::1:2:3:4:5:6:7
        @"::)" +                                               // ::
        @"(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))?$")]      // /128 (Optional)
    public string IP {get; set;}

    protected override void ProcessRecord()
    {
        if (string.IsNullOrEmpty(IP))
            WriteObject(IPv6Address.GetRandom());
        else
            WriteObject(IPv6Address.Parse(IP));
    }
}