using System;

namespace Worker369.Utility;

public sealed class Style
{
    private readonly static Lazy<Style> _lazy = new(() => new Style());

    public readonly static Style Instance = _lazy.Value;

    public string Bold      {get;} = "\x1b[1m";
    public string Underline {get;} = "\x1b[4m";
    public string Dim       {get;} = "\x1b[2m";
    public string Reset     {get;} = "\x1b[0m";

    public ForegroundColor Foreground {get;}

    private Style ()
    {
        Foreground = new ForegroundColor();
    }

    public class ForegroundColor
    {
        internal ForegroundColor () {}

        public string BrightCyan    {get;} = "\x1b[96m";
        public string BrightGreen   {get;} = "\x1b[92m";
        public string BrightMagenta {get;} = "\x1b[95m";
        public string Green         {get;} = "\x1b[32m";
    }
}