using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Show, "LineNumber", DefaultParameterSetName = "None")]
[Alias("line")]
[OutputType(typeof(LineInfo))]
public class LineNumber_Show : PSCmdlet
{
    private List<LineInfo> _lines;
    private int _processed;

    [Parameter(Position = 0, ValueFromPipeline = true)]
    [AllowEmptyString]
    public string[] Input { get; set; }

    [Parameter(ParameterSetName = "Range")]
    [ValidatePattern(@"^[1-9]+[0-9]*\.\.[1-9]+[0-9]*$")]
    public string Range { get; set; }

    protected override void BeginProcessing()
    {
        _lines = new List<LineInfo>();
        _processed = 0;
    }

    protected override void ProcessRecord()
    {
        var lines = Input.SelectMany(input => input.Split(new string[] { "\n" }, StringSplitOptions.None));

        foreach (var line in lines)
            _lines.Add(new LineInfo(++_processed, line));
    }

    protected override void EndProcessing()
    {
        // If no range is specified, print all lines.
        if (ParameterSetName == "None")
        {
            WriteObject(_lines, true);
            return;
        }

        // Interprete the specified range.
        var numbers = Range.Split(new string[] { ".." }, StringSplitOptions.None);
        var num1 = Convert.ToInt32(numbers[0]);
        var num2 = Convert.ToInt32(numbers[1]);

        var start = Math.Min(num1, num2) - 1;
        var length = Math.Abs(num1 - num2) + 1;

        // Exit early if the whole range falls outside the list.
        if (start >= _lines.Count) return;

        // Trim the range if the it exceed the size of the list.
        if (start + length >= _lines.Count) length = _lines.Count - start;

        // Extract out the range.
        var range = _lines.GetRange(start, length);

        // Reverse the range is needed.
        if (num1 > num2) range.Reverse();

        // Print the lines.
        WriteObject(range, true);
    }
}