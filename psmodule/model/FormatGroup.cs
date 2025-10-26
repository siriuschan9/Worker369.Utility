namespace Worker369.Utility;

public class FormatGroup
{
    public string Scaled   {get; set;}
    public string Unscaled {get; set;}
    public string Fraction {get; set;}

    private FormatGroup() => Reset();

    internal static FormatGroup MakeDefault() => new();

    internal void Reset()
    {
        Scaled   = "N2";
        Unscaled = "N0";
        Fraction = "G3";
    }
}