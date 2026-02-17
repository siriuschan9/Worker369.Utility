using System.Management.Automation;
using System.Reflection.Metadata;
using System.Text.Json;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Format, "Json")]
[Alias("json")]
public class Json_Format : PSCmdlet
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    [Parameter(ValueFromPipeline = true)]
    public string Input {get; set; }

    protected override void BeginProcessing()
    {
        base.BeginProcessing();
    }
    protected override void ProcessRecord()
    {
        JsonDocument json_doc;
        try
        {
            json_doc = JsonDocument.Parse(Input);
        }
        catch
        {
            WriteObject(Input);
            return;
        }
        WriteObject(JsonSerializer.Serialize(json_doc.RootElement, _options));
    }
}