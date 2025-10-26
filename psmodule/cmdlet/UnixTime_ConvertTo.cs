using System;
using System.Management.Automation;

namespace Worker369.Utility;

[Cmdlet(VerbsData.ConvertTo, "UnixTime")]
[Alias("to_epoch")]
[OutputType(typeof(long))]
public class UnixTime_ConvertTo : PSCmdlet
{
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public DateTime DateTime { get; set; } = DateTime.Now;

    [Parameter()]
    [Alias("ms")]
    public SwitchParameter Milliseconds { get; set; }

    protected override void ProcessRecord()
    {
        try
        {
            if (Milliseconds.IsPresent)
                WriteObject(((DateTimeOffset)DateTime).ToUnixTimeMilliseconds());
            else
                WriteObject(((DateTimeOffset)DateTime).ToUnixTimeSeconds());
        }
        catch (ArgumentException ex)
        {

            var error_record = new ErrorRecord(
                ex,
                errorId: "DateTimeOutOfRange",
                errorCategory: ErrorCategory.InvalidArgument,
                DateTime
            );
            WriteError(error_record);
        }
        catch (Exception ex)
        {

            var error_record = new ErrorRecord(
                ex,
                errorId: "UnixTimeConversionFailed",
                errorCategory: ErrorCategory.NotSpecified,
                DateTime
            );
            WriteError(error_record);
        }
    }
}