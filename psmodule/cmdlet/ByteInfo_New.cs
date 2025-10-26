using System;
using System.Linq;
using System.Management.Automation;
using System.Numerics;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.New, "ByteInfo", DefaultParameterSetName = "None")]
[Alias("byte")]
[OutputType(typeof(ByteInfo))]
public class ByteInfo_New : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public BigInteger Value { get; set; }

    [Parameter(ParameterSetName = "DisplayUnit")]
    [ArgumentCompleter(typeof(ByteInfo_DisplayUnitCompleter))]

    public string DisplayUnit { get; set; }

    [Parameter(ParameterSetName = "None", Position = 1)]
    public ByteMetric MetricSystem { get; set; } = ByteMetric.IEC;

    protected override void ProcessRecord()
    {
        if (ParameterSetName == "DisplayUnit" && !ValidateDisplayUnit(DisplayUnit))
        {
            var exception = new ArgumentException(
                message: "Invalid DisplayUnit",
                paramName: nameof(DisplayUnit)
            );

            var error_record = new ErrorRecord(
                exception:     exception,
                errorId:       "InvalidDisplayUnit",
                errorCategory: ErrorCategory.InvalidArgument,
                targetObject:  null
            );

            ThrowTerminatingError(error_record);
        }

        var new_byte = MyInvocation.BoundParameters.ContainsKey("DisplayUnit")
            ? new ByteInfo(Value, DisplayUnit)
            : new ByteInfo(Value, MetricSystem);

        WriteObject(new_byte);
    }

    private static bool ValidateDisplayUnit(string unit)
    {
        var iec_abbreviations = UnitsProvider.ByteUnitAbbreviations_IEC.GetUnits();
        var iec_names         = UnitsProvider.ByteUnitNames_IEC.GetUnits();
        var si_abbreviations  = UnitsProvider.ByteUnitAbbreviations_SI.GetUnits();
        var si_names          = UnitsProvider.ByteUnitNames_SI.GetUnits();

        var valid_units = iec_abbreviations
            .Union(iec_names).Union(si_abbreviations).Union(si_names);

        return valid_units.Contains(unit, StringComparer.InvariantCultureIgnoreCase);
    }
}