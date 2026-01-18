using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Format, "Html")]
[Alias("fhtm")]
public class Html_Format : PSCmdlet
{
    private List<PSObject> _objects;

    [Parameter(ValueFromPipeline = true)]
    public PSObject InputObject { get; set;}

    [Parameter(Position = 0)]
    public string[] Column {get; set;}

    [Parameter()]
    public string GroupBy {get; set;}

    protected override void BeginProcessing()
    {
        _objects = [];
    }

    protected override void ProcessRecord()
    {
        if (InputObject != null) _objects.Add(InputObject);
    }

    protected override void EndProcessing()
    {
        // If no objects have been collected, exit early.
        if (_objects.Count == 0) return;

        // Initialize stringbuilder for building html.
        var html = new StringBuilder();

        // Use the first object in the pipeline to define the columns.
        IList<string> column_names = Column is null || Column.Length == 0
            ? GetColumnNames(_objects[0])
            : [.. Column];

        if (!string.IsNullOrEmpty(GroupBy))
        {
            // Don't print out the column used to group the objects.
            column_names.Remove(GroupBy);

            // Assign items into their respective groups.
            var groups = _objects.GroupBy(obj => GetPropertyValue(obj.Properties[GroupBy]));

            // Build the top part of the html document.
            BeginHtml(html);

            // Build one html table for each group.
            foreach (var group in groups)
            {
                AppendHtmlTable(
                    html: html,
                    group_name: GroupBy,
                    group_value: group.Key.ToString(),
                    objects: group, column_names: column_names
                );
            }
            // Finish the html document.
            EndHtml(html);
        }
        else
        {
            // Build the top part of the html document.
            BeginHtml(html);

            // Build one html table for each group.
            AppendHtmlTable(
                html: html,
                group_name: null,
                group_value: null,
                objects: _objects, column_names: column_names
            );

            // Finish the html document.
            EndHtml(html);
        }

        WriteObject(html.ToString());
    }

    private static IList<string> GetColumnNames(PSObject obj)
    {
        return [.. obj.Properties.Select(p => p.Name)];
    }

    private static object GetPropertyValue(PSPropertyInfo pi)
    {
        return (pi?.Value as PSObject)?.BaseObject ?? pi?.Value;
    }

    private static void BeginHtml(StringBuilder html)
    {
        html.AppendLine("""
        <html>
        <head>
            <style>
                table.outer {
                    width: 100%;
                    border-spacing: 0;
                    border-collapse: collapse;
                }
                table.inner {
                    border-spacing: 0;
                    border-collapse: collapse;
                    font-family: monaco;
                    font-size: 75%;
                    line-height: 2em;
                    letter-spacing: -0.3;
                }
                table.inner thead tr.grp-head th {
                    text-align: left;
                    padding: 1em 0.33em;
                    color: #505050;
                    font-weight: normal;
                    border-bottom: solid 1px #e3e3e3;
                }
                table.inner thead tr.col-head th {
                    text-align: left;
                    padding: 0.33em 1em;
                    font-weight: normal;
                    background-color: #f8f8f8;
                    color: #761a1a;
                    border-bottom: solid 1px #e3e3e3;
                }
                table.inner tbody tr td{
                    padding: 0.33em 1em;
                    vertical-align: top;
                    color: #505050;
                    white-space: pre-wrap;
                    border-bottom: solid 1px #e3e3e3;
                }
                table.inner tbody tr.last td{
                    padding: 0.33em 1em;
                    border-bottom: none;
                }
                span.grp-name {
                    font-weight: normal;
                    color: #761a1a;
                }
                span.hint {
                    font-size: 90%;
                    text-decoration: underline;
                    color: #537791;
                }
            </style>
        </head>
        <body>
            <table class="outer">
                <tr>
                    <td>
                        <table class="inner">
        """);
    }

    private static void EndHtml(StringBuilder html)
    {
        html.AppendLine("""
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """);
    }

    private static void AppendHtmlTable(
        StringBuilder html,
        string group_name,
        string group_value,
        IEnumerable<PSObject> objects,
        IEnumerable<string> column_names)
    {
        int num_col = column_names.Count();

        html.AppendLine(new string(' ', 20) + """<thead>""");
        if (!string.IsNullOrEmpty(group_name) && !string.IsNullOrEmpty(group_value))
        {
            html.AppendLine(new string(' ', 24) + """<tr class="grp-head">""");
            html.AppendLine(new string(' ', 28) +
                $"""<th colspan="{num_col}" class="grp-head"><span class="grp-name">{group_name} :</span> {group_value}"""
            );
            html.AppendLine(new string(' ', 24) + """</tr>""");
        }
        html.AppendLine(new string(' ', 24) + """<tr class="col-head">""");
        foreach (string name in column_names)
            html.AppendLine(new string(' ', 28) + $"""<th>{name}</th>""");
        html.AppendLine(new string(' ', 24) + """</tr>""");
        html.AppendLine(new string(' ', 20) + """</thead>""");
        html.AppendLine(new string(' ', 20) + """<tbody>""");
        foreach (var obj in objects)
        {
            html.AppendLine(new string(' ', 24) + """<tr>""");
            foreach (string name in column_names)
            {
                var items = GetItems(obj.Properties[name].Value);
                html.AppendLine(new string(' ', 28) + $"""<td>{string.Join("<br>", items)}</td>""");
            }
            html.AppendLine(new string(' ', 24) + """</tr>""");
        }
        html.AppendLine(new string(' ', 24) + """<tr class="last">""");
        html.AppendLine(new string(' ', 28) + """<td>&nbsp</td>""");
        html.AppendLine(new string(' ', 24) + """</tr>""");
        html.AppendLine(new string(' ', 20) + """</tbody>""");
    }

    // Convert object to collection.
    private static IEnumerable<object> GetItems(object value)
    {
        // Unbox if value is PSObject.
        object unboxed = (value as PSObject)?.BaseObject ?? value;

        if (unboxed is null) yield break;

        else if (unboxed is string) yield return value;

        else if (unboxed is IEnumerable items)
            foreach (object item in items) yield return item;

        else yield return value;
    }
}