using System;
using System.Linq;
using System.Management.Automation;
using System.Numerics;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "NumberInfo", DefaultParameterSetName = "None")]
[Alias("number")]
[OutputType(typeof(NumberInfo))]
public class NumberInfo_New : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public BigInteger Value { get; set; }

    [Parameter(ParameterSetName = "DisplayUnit")]
    [ArgumentCompleter(typeof(NumberInfo_DisplayUnitCompleter))]
    public string DisplayUnit { get; set; }

    [Parameter()]
    public NumberInfoSettings FormatSettings { get; set; }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "DisplayUnit" && !ValidateDisplayUnit(DisplayUnit))
        {
            var exception = new ArgumentException(
                message: "Invalid DisplayUnit",
                paramName: nameof(DisplayUnit)
            );

            var error_record = new ErrorRecord(
                exception: exception,
                errorId: "InvalidDisplayUnit",
                errorCategory: ErrorCategory.InvalidArgument,
                targetObject: null
            );

            ThrowTerminatingError(error_record);
        }

        var has_display_unit    = MyInvocation.BoundParameters.ContainsKey("DisplayUnit");
        var has_format_settings = MyInvocation.BoundParameters.ContainsKey("FormatSettings");

        NumberInfo number = new(Value)
        {
            DisplayUnit    = has_display_unit    ? DisplayUnit    : null,
            FormatSettings = has_format_settings ? FormatSettings : null
        };

        WriteObject(number);
    }

    private static bool ValidateDisplayUnit(string unit)
    {
        return
            UnitsProvider.NumericUnitNames.GetUnits()
                .Contains(unit, StringComparer.InvariantCultureIgnoreCase) ||
            UnitsProvider.NumericUnitAbbreviations.GetUnits()
                .Contains(unit, StringComparer.InvariantCultureIgnoreCase);
    }
}