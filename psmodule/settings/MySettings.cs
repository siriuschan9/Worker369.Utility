namespace Worker369.Utility;

public sealed class MySettings
{
    public readonly static MySettings Instance = new();

    public ByteInfoSettings     ByteInfo     { get; } = ByteInfoSettings.Instance;
    public NumberInfoSettings   NumberInfo   { get; } = NumberInfoSettings.Instance;
    public FormatColumnSettings FormatColumn { get; } = FormatColumnSettings.Instance;
    public IPv4AddressSettings  IPv4Address  { get; } = IPv4AddressSettings.Instance;
    public IPv6AddressSettings  IPv6Address  { get; } = IPv6AddressSettings.Instance;

    private MySettings(){}

    public void Reset()
    {
        ByteInfo.Reset();
        NumberInfo.Reset();
        FormatColumn.Reset();
        IPv4Address.Reset();
        IPv6Address.Reset();
    }
}