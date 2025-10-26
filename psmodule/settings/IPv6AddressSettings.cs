namespace Worker369.Utility;

public sealed class IPv6AddressSettings
{
    public readonly static IPv6AddressSettings Instance = new();

    public IPv6AddressDisplay DisplayStyle { get; set; }
    public string NetworkBitsStyle { get; set; }
    public string HostBitsStyle { get; set; }

    private IPv6AddressSettings() => Reset();

    public void Reset()
    {
        DisplayStyle = IPv6AddressDisplay.IP;
        NetworkBitsStyle = "\x1b[95m";            // Bright Magenta
        HostBitsStyle = "\x1b[96m";            // Bright Cyan
    }
}