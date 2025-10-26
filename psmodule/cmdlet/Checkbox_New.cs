using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "Checkbox")]
[Alias("checkbox")]
[OutputType(typeof(Checkbox))]
public class Checkbox_New : PSCmdlet
{
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public bool? IsChecked { get; set; }

    [Parameter()]
    [AllowEmptyString()]
    public string Description { get; set; }

    [Parameter()]
    public SwitchParameter PlainText { get; set; } = false;

    protected override void ProcessRecord()
    {
        WriteObject(new Checkbox(IsChecked, Description, PlainText.IsPresent));
    }
}
