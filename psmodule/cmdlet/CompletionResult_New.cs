using System;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "CompletionResult")]
[OutputType(typeof(CompletionResult))]
public class CompletionResult_New : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string CompletionText { get; set; }

    [Parameter(Mandatory = true)]
    public string ListItemText { get; set; }

    [Parameter(Mandatory = true)]
    public CompletionResultType ResultType { get; set; } = CompletionResultType.ParameterValue;

    [Parameter(Mandatory = true)]
    public string ToolTip { get; set; }

    protected override void ProcessRecord()
    {
        var completion_result = new CompletionResult(
            completionText: CompletionText,
            listItemText: ListItemText,
            ResultType,
            ToolTip
        );
        WriteObject(completion_result);
    }
}