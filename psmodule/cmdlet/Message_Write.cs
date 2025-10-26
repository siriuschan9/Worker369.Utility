using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Worker369.Utility;

[Cmdlet(VerbsCommunications.Write, "Message")]
public class Message_Write : PSCmdlet
{
    [Parameter(ParameterSetName = "Progress", Mandatory = true)]
    public SwitchParameter Progress { get; set; }

    [Parameter(ParameterSetName = "Output", Mandatory = true)]
    public SwitchParameter Output { get; set; }

    [Parameter(ParameterSetName = "Information", Mandatory = true)]
    public SwitchParameter Information { get; set; }

    [Parameter(ParameterSetName = "Progress", Mandatory = true, Position = 0)]
    public string Activity { get; set; }

    [Parameter(Mandatory = true, Position = 1)]
    public string Message { get; set; }

    protected override void ProcessRecord()
    {
        bool is_verbose = MyInvocation.BoundParameters.TryGetValue("Verbose", out object verbose) &&
            (SwitchParameter)verbose == SwitchParameter.Present;

        is_verbose |= (ActionPreference)GetVariableValue("VerbosePreference") == ActionPreference.Continue;

        if (is_verbose)
        {
            WriteVerbose(Message);
            return;
        }

        switch (ParameterSetName)
        {
            case "Progress":
                WriteProgress(new ProgressRecord(activityId: 1, Activity, Message)); break;

            case "Output":
                WriteObject(Message); break;

            case "Information":
                WriteInformation(Message, tags: null); break;

            default: break;
        }
    }
}