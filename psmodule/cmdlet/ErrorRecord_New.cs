
using System;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "ErrorRecord")]
[OutputType(typeof(ErrorRecord))]
public class ErrorRecord_New : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    public string ErrorMessage { get; set; }

    [Parameter(Mandatory = true, Position = 1)]
    public string ErrorId { get; set; }

    [Parameter(Position = 2)]
    public ErrorCategory ErrorCategory { get; set; } = ErrorCategory.NotSpecified;

    [Parameter()]
    public object TargetObject { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject(new ErrorRecord(
            new Exception(ErrorMessage),
            ErrorId,
            ErrorCategory,
            TargetObject
        ));
    }
}
