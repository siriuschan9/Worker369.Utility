using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "IPv4SubnetMask")]
[Alias("mask")]
[OutputType(typeof(IPv4SubnetMask))]
public class IPv4SubnetMask_New : PSCmdlet
{
    [Parameter(ParameterSetName = "PrefixLength", Position = 0, ValueFromPipeline = true)]
    [ValidateRange(0, 32)]
    public int? PrefixLength {get; set;}

    protected override void ProcessRecord()
    {
        if (PrefixLength.HasValue)
            WriteObject(new IPv4SubnetMask((int)PrefixLength));
        else
            WriteObject(
                IPv4SubnetMask.ListAll(),  // sendToPipeline
                true);                     // enumerateCollection
    }
}