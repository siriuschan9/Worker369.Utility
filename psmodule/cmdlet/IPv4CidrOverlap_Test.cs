using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsDiagnostic.Test, "IPv4CidrOverlap", DefaultParameterSetName = "String")]
[Alias("overlap?")]
[OutputType(typeof(bool))]
public class CidrOverlap_Test : PSCmdlet
{
    [Parameter(Position = 0, Mandatory = true, ParameterSetName = "String")]
    [ValidatePattern(
        @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}" + // 255.255.255.
        @"([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])" +         // 255
        @"\/([0-9]|[1-2][0-9]|3[0-2])$")]                               // /32
    public string Cidr1 { get; set; }

    [Parameter(Position = 1, Mandatory = true, ParameterSetName = "String")]
    [ValidatePattern(
        @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}" + // 255.255.255.
        @"([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])" +         // 255
        @"\/([0-9]|[1-2][0-9]|3[0-2])$")]                               // /32
    public string Cidr2 { get; set; }

    [Parameter(Position = 0, Mandatory = true, ParameterSetName = "IPv4Subnet")]
    public IPv4Subnet Subnet1 { get; set; }

    [Parameter(Position = 1, Mandatory = true, ParameterSetName = "IPv4Subnet")]
    public IPv4Subnet Subnet2 { get; set; }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "String")
        {
            WriteObject(IPv4Subnet.IsOverlapping(IPv4Subnet.Parse(Cidr1), IPv4Subnet.Parse(Cidr2)));
            return;
        }

        if (ParameterSetName == "IPv4Subnet")
        {
            WriteObject(IPv4Subnet.IsOverlapping(Subnet1, Subnet2));
            return;
        }
    }
}