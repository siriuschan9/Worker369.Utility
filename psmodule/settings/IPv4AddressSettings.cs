namespace Worker369.Utility
{
    public sealed class IPv4AddressSettings
    {
        public readonly static IPv4AddressSettings Instance = new();

        public IPv4AddressDisplay DisplayStyle     { get; set; }
        public string             NetworkBitsStyle { get; set; }
        public string             HostBitsStyle    { get; set; }

        private IPv4AddressSettings() => Reset();

        public void Reset()
        {
            DisplayStyle     = IPv4AddressDisplay.IP;
            NetworkBitsStyle = "\x1b[95m";            // Bright Magenta
            HostBitsStyle    = "\x1b[96m";            // Bright Cyan
        }
    }
}