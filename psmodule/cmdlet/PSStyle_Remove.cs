using System.Management.Automation;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Remove, "PSStyle")]
[OutputType(typeof(string))]
[Alias("style_rm")]
public class PSStyle_Remove : PSCmdlet
{
    private static readonly string _style_pattern = @"\e\[\d{1,3}(;\d{1,3})*m";

    [Parameter(Position = 0, ValueFromPipeline = true)]
    [AllowEmptyString, AllowNull]
    public string Input {get; set;}

    protected override void ProcessRecord()
    {
        if (Input is null) return;

        WriteObject(Regex.Replace(Input, _style_pattern, string.Empty));
    }
}