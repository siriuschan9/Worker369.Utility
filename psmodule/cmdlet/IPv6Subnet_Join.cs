using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Join, "IPv6Subnet", DefaultParameterSetName = "String")]
[Alias("join6")]
[OutputType(typeof(IPv6Subnet))]
public class IPv6Subnet_Join : PSCmdlet
{
    private List<IPv6Subnet> subnet_list;

    [Parameter(ParameterSetName = "IPv6Subnet", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public IPv6Subnet[] Subnet { get; set; }

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
    public string[] Cidr { get; set; }

    protected override void BeginProcessing()
    {
        subnet_list = new List<IPv6Subnet>();
    }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "IPv6Subnet")
        {
            subnet_list.AddRange(Subnet);
            return;
        }

        if (ParameterSetName == "String")
        {
            subnet_list.AddRange(
                Cidr.Select(cidr => IPv6Subnet.Parse(cidr))
            );
            return;
        }
    }

    protected override void EndProcessing()
    {
        WriteObject(IPv6Subnet.Merge(subnet_list.ToArray()));
    }
}