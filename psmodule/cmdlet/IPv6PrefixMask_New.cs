using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "IPv6PrefixMask")]
[Alias("mask6")]
[OutputType(typeof(IPv6PrefixMask))]
public class IPv6PrefixMask_New : PSCmdlet
{
    [Parameter(ParameterSetName = "PrefixLength", Position = 0, ValueFromPipeline = true)]
    [ValidateRange(0, 128)]
    public int? PrefixLength { get; set; }

    protected override void ProcessRecord()
    {
        if (PrefixLength.HasValue)
            WriteObject(new IPv6PrefixMask((int)PrefixLength));
        else
            WriteObject(
                IPv6PrefixMask.ListAll(), // sendToPipeline
                true);                    // enumerateCollection
    }
}