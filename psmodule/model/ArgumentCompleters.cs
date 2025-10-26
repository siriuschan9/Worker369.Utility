using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Worker369.Utility;

public class ByteInfo_DisplayUnitCompleter : IArgumentCompleter
{
    public IEnumerable<CompletionResult> CompleteArgument(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters)
    {
        foreach (var unit in UnitsProvider.ByteUnitAbbreviations_IEC.GetUnits()
            .Where(u => u.StartsWith(wordToComplete)))
            yield return new CompletionResult(unit);

        foreach (var unit in UnitsProvider.ByteUnitAbbreviations_SI.GetUnits()
            .Where(u => u.StartsWith(wordToComplete)))
            yield return new CompletionResult(unit);

        foreach (var unit in UnitsProvider.ByteUnitNames_IEC.GetUnits()
            .Where(u => u.StartsWith(wordToComplete)))
            yield return new CompletionResult(unit);

        foreach (var unit in UnitsProvider.ByteUnitNames_SI.GetUnits()
            .Where(u => u.StartsWith(wordToComplete)))
            yield return new CompletionResult(unit);
    }
}

public class NumberInfo_DisplayUnitCompleter : IArgumentCompleter
{
    public IEnumerable<CompletionResult> CompleteArgument(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters)
    {
        foreach (var unit in UnitsProvider.NumericUnitAbbreviations.GetUnits()
            .Where(u => !string.IsNullOrEmpty(u) && u.StartsWith(wordToComplete)))
            yield return new CompletionResult(unit);

        foreach (var unit in UnitsProvider.NumericUnitNames.GetUnits()
            .Where(u => !string.IsNullOrEmpty(u) && u.StartsWith(wordToComplete)))
            yield return new CompletionResult(unit);
    }
}