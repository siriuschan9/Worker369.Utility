using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Join, "IPv4Subnet", DefaultParameterSetName = "String")]
[Alias("join")]
[OutputType(typeof(IPv4Subnet))]
public class IPv4Subnet_Join : PSCmdlet
{
    private List<IPv4Subnet>subnet_list;

    [Parameter(ParameterSetName = "IPv4Subnet", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public IPv4Subnet[] Subnet {get; set;}

    [Parameter(ParameterSetName = "String", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    [ValidatePattern(
        @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}" + // 255.255.255.
        @"([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])" +         // 255
        @"\/([0-9]|[1-2][0-9]|3[0-2])$")]                               // /32
    public string[] Cidr { get; set;}

    protected override void BeginProcessing()
    {
        subnet_list = new List<IPv4Subnet>();
    }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "IPv4Subnet")
        {
            subnet_list.AddRange(Subnet);
            return;
        }

        if (ParameterSetName == "String")
        {
            subnet_list.AddRange(
                Cidr.Select(cidr => IPv4Subnet.Parse(cidr))
            );
            return;
        }
    }

    protected override void EndProcessing()
    {
        WriteObject(IPv4Subnet.Merge(subnet_list.ToArray()));
    }
}