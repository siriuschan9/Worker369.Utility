namespace Worker369.Utility
{
    public sealed class NumberInfoSettings
    {
        public readonly static NumberInfoSettings Instance = new();

        public FormatGroup   Format       { get;      } = FormatGroup.MakeDefault();
        public NumberDisplay DisplayStyle { get; set; }

        private NumberInfoSettings() => Reset();

        public void Reset()
        {
            Format.Reset();
            DisplayStyle = NumberDisplay.None;
        }

        public static NumberInfoSettings Make() => new();
    }
}