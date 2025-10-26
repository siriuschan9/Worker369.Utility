namespace Worker369.Utility;

public sealed class ByteInfoSettings
{
    public readonly static ByteInfoSettings Instance = new();

    public FormatGroup Format { get; } = FormatGroup.MakeDefault();
    public NumberDisplay DisplayStyle { get; set; }
    public ByteMetric DefaultMetricSystem { get; set; }

    private ByteInfoSettings() => Reset();

    public void Reset()
    {
        Format.Reset();
        DisplayStyle = NumberDisplay.HumanReadable;
        DefaultMetricSystem = ByteMetric.IEC;
    }
}