using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Worker369.Utility;

[Cmdlet(VerbsDiagnostic.Test, "IsValidIPv4")]
[Alias("is_ipv4?")]
[OutputType(typeof(bool))]
public class IsValidIPv4_Test : PSCmdlet
{
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string Input { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject(Regex.IsMatch(Input, IPPattern.IPv4Address) || Regex.IsMatch(Input, IPPattern.IPv4Subnet));
    }
}