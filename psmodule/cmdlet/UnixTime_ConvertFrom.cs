using System;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsData.ConvertFrom, "UnixTime")]
[Alias("from_epoch")]
[OutputType(typeof(DateTime))]
public class UnixTime_ConvertFrom : PSCmdlet
{
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public long UnixTime { get; set; } = 0;

    [Parameter()]
    [Alias("ms")]
    public SwitchParameter Milliseconds { get; set; }

    [Parameter()]
    public SwitchParameter Utc { get; set; }

    protected override void ProcessRecord()
    {
        try
        {
            var dt_offset = Milliseconds.IsPresent
                ? DateTimeOffset.FromUnixTimeMilliseconds(UnixTime)
                : DateTimeOffset.FromUnixTimeSeconds(UnixTime);

            if (Utc.IsPresent)
                WriteObject(dt_offset.ToUniversalTime().DateTime);
            else
                WriteObject(dt_offset.ToLocalTime().DateTime);
        }
        catch (ArgumentException ex)
        {

            var error_record = new ErrorRecord(
                ex,
                errorId: "UnixTimeOutOfRange",
                errorCategory: ErrorCategory.InvalidArgument,
                UnixTime
            );
            WriteError(error_record);
        }
        catch (Exception ex)
        {

            var error_record = new ErrorRecord(
                ex,
                errorId: "UnixTimeConversionFailed",
                errorCategory: ErrorCategory.NotSpecified,
                UnixTime
            );
            WriteError(error_record);
        }
    }
}