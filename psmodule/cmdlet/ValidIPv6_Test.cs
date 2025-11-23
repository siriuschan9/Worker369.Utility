using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Worker369.Utility;

[Cmdlet(VerbsDiagnostic.Test, "IsValidIPv6")]
[Alias("is_ipv6?")]
[OutputType(typeof(bool))]
public class IsValidIPv6_Test : PSCmdlet
{
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string Input { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject(Regex.IsMatch(Input, IPPattern.IPv6Address) || Regex.IsMatch(Input, IPPattern.IPv6Subnet));
    }
}