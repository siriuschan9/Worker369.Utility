using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Split, "IPv4Subnet", DefaultParameterSetName = "String")]
[Alias("split")]
[OutputType(typeof(IPv4Subnet[]))]
public class IPv4Subnet_Split : PSCmdlet
{
    [Parameter(ParameterSetName = "IPv4Subnet", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public IPv4Subnet Subnet {get; set;}

    [Parameter(ParameterSetName = "String", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    [ValidatePattern(
        @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}" + // 255.255.255.
        @"([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])" +         // 255
        @"\/([0-9]|[1-2][0-9]|3[0-2])$")]                               // /32
    public string Cidr { get; set;}

    [Parameter(Position = 1)]
    [ValidateRange(0, 33)]
    public int Bits { get; set; } = 1;

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "IPv4Subnet")
        {
            WriteObject(IPv4Subnet.Split(Subnet, Bits), true);
            return;
        }

        if (ParameterSetName == "String")
        {
            WriteObject(IPv4Subnet.Split(IPv4Subnet.Parse(Cidr), Bits), true);
            return;
        }
    }
}