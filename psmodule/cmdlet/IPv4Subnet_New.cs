using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "IPv4Subnet")]
[Alias("subnet")]
[OutputType(typeof(IPv4Subnet))]
public class IPv4Subnet_New : PSCmdlet
{
    [Parameter(Position = 0, ValueFromPipeline = true)]
    [ValidatePattern(
        @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}" + // 255.255.255.
        @"([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])" +         // 255
        @"\/([0-9]|[1-2][0-9]|3[0-2])$")]                               // /32
    public string Cidr { get; set; }

    protected override void ProcessRecord()
    {
        if (string.IsNullOrEmpty(Cidr))
            WriteObject(IPv4Subnet.GetRandom());
        else
            WriteObject(IPv4Subnet.Parse(Cidr));
    }
}