using System.Collections;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Pop, "ErrorRecord")]
public class ErrorRecord_Pop : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public ErrorRecord ErrorRecord { get; set; }

    protected override void ProcessRecord()
    {
        var error_list = (ArrayList)SessionState.PSVariable.GetValue("global:Error");

        if (error_list.Count == 0) return;

        var last_error = error_list[0] as ErrorRecord;
        var this_error = ErrorRecord;

        if (last_error.Exception == this_error.Exception) error_list.RemoveAt(0);
    }
}