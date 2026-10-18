using System;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Remove, "UrlEncoding")]
[OutputType(typeof(string))]
[Alias("urlen_rm")]
public class UrlEncoding_Remove : PSCmdlet
{
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public string InputText { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject(System.Web.HttpUtility.UrlDecode(InputText));
    }
}