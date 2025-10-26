using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "IPv4Address")]
[Alias("ip")]
[OutputType(typeof(IPv4Address))]
public class IPv4Address_New : PSCmdlet
{
    [Parameter(Position = 0, ValueFromPipeline = true)]
    [ValidatePattern(
        @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}" + // 255.255.255.
        @"([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])" +         // 255
        @"(\/([0-9]|[1-2][0-9]|3[0-2]))?$")]                            // /32 (Optional)
    public string IP {get; set;}

    protected override void ProcessRecord()
    {
        if (string.IsNullOrEmpty(IP))
            WriteObject(IPv4Address.GetRandom());
        else
            WriteObject(IPv4Address.Parse(IP));
    }
}