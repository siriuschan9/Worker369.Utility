using System.Speech.Synthesis;

namespace Worker369.Utility;

public sealed class CheckboxSettings
{
    public readonly static CheckboxSettings Instance = new();

    public char CheckedChar {get; set; }
    public char UncheckedChar {get; set;}
    public char NoValueChar {get; set;}

    private CheckboxSettings() => Reset();

    public void Reset()
    {
        CheckedChar = '✓';
        UncheckedChar = ' ';
        NoValueChar = '?';
    }
}

/*
○   9675    25cb
●   9679    25cf
✓   10003   2713
*/