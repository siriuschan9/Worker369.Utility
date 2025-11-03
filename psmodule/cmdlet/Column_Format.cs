using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;

namespace Worker369.Utility;

[Cmdlet(VerbsCommon.Format, "Column")]
[Alias("fcol")]
public class Column_Format : PSCmdlet
{
    private static readonly string _style_pattern = @"\e\[\d{1,3}(;\d{1,3})*m";
    private static readonly Regex  _style_regex =
        new(_style_pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly FormatColumnSettings _settings = FormatColumnSettings.Instance;
    private static readonly string _reset_style  = "\x1b[0m";

    private string         _header_style;
    private string         _border_style;
    private List<PSObject> _objects;

    [Parameter(ValueFromPipeline = true)]
    public PSObject InputObject {get; set;}

    [Parameter(Position = 0)]
    public string[] Column {get; set;}

    [Parameter()]
    public string GroupBy {get; set;}

    [Parameter()]
    public SwitchParameter PlainText {get; set;}

    [Parameter()]
    public SwitchParameter NoRowSeparator {get; set;}

    [Parameter()]
    public string[] AlignRight {get; set;}

    [Parameter()]
    public string[] AlignLeft {get; set;}

    protected override void BeginProcessing()
    {
        // Initialize object list, column list, and width dictionary.
        _objects = new List<PSObject>();

        // Strip out ANSI codes in style if caller specified -IsPlainText option.
        _header_style = PlainText ? string.Empty : _settings.HeaderStyle;
        _border_style = PlainText ? string.Empty : _settings.BorderStyle;
    }

    protected override void ProcessRecord()
    {
        // Collect objects from the pipeline.
        if (InputObject != null) _objects.Add(InputObject);
    }

    protected override void EndProcessing()
    {
        // If no objects collected, exit early.
        if (_objects.Count == 0) return;

        // Find out the list of columns to print out.
        IList<string> column_names = Column is null || Column.Length == 0
            ? GetColumnNames(_objects[0])
            : Column.ToList();

        // Determine the column widths needed,
        IDictionary<string, int> column_widths = GetColumnWidths(
            objects             : _objects,
            column_names        : column_names,
            align_right_columns : AlignRight,
            align_left_columns  : AlignLeft
        );

        if (!string.IsNullOrEmpty(GroupBy))
        {
            // Don't print out the column used to group the objects.
            column_names.Remove(GroupBy);

            WriteObject(string.Empty);
            //Console.WriteLine();

            var groups = _objects.GroupBy(obj => GetPropertyValue(obj.Properties[GroupBy]));

            foreach (var group in groups)
            {
                WriteObject($"{_header_style}{GroupBy} : {_reset_style}{group.Key}");
                WriteObject(string.Empty);
                //Console.WriteLine($"{_header_style}{GroupBy} : {_reset_style}{group.Key}");
                //Console.WriteLine();
                PrintColumns(
                    cmdlet            : this,
                    objects           : group,
                    column_names      : column_names,
                    column_widths     : column_widths,
                    is_indent         : true,
                    has_row_separator : !NoRowSeparator,
                    header_style      : _header_style,
                    border_style      : _border_style,
                    reset_style       : _reset_style
                );
                WriteObject(string.Empty);
                //Console.WriteLine();
            }
        }
        else
        {
            WriteObject(string.Empty);
            //Console.WriteLine();
            PrintColumns(
                cmdlet            : this,
                objects           : _objects,
                column_names      : column_names,
                column_widths     : column_widths,
                is_indent         : false,
                has_row_separator : !NoRowSeparator,
                header_style      : _header_style,
                border_style      : _border_style,
                reset_style       : _reset_style
            );
            WriteObject(string.Empty);
            //Console.WriteLine();
        }
    }

    private static IList<string> GetColumnNames(PSObject obj)
    {
        return obj.Properties.Select(p => p.Name).ToList();
    }

    private static Dictionary<string, int> GetColumnWidths(
        IEnumerable<PSObject> objects,
        IEnumerable<string> column_names,
        IEnumerable<string> align_right_columns,
        IEnumerable<string> align_left_columns
    ) {
        // 1. Initialize a column width dictionary.
        var widths = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        // 2. Start all entries with the lengths of the column names.
        foreach (string column_name in column_names)
            widths[column_name] = column_name.Length;

        Debug.Assert(
            widths.Keys.All(
                k => column_names.Contains(k, StringComparer.InvariantCultureIgnoreCase)));

        // 3. Go through the objects to adjust the widths of the columns if we have longer items.
        foreach (PSObject obj in objects)
        {
            // Grab only the list of properties whose name is defined in the column_names list.
            var properties = obj.Properties
                .Where(p => column_names.Contains(p.Name, StringComparer.InvariantCultureIgnoreCase));

            foreach (PSPropertyInfo p in properties)
            {
                // Go through each item in the property's collection to check for longer item.
                foreach (object item in GetItems(p.Value))
                {
                    int item_width    = $"{item}".Length;
                    int current_width = widths[p.Name];

                    // Account for unprintable chars (style codes) - substract the number from the width.
                    var matches = _style_regex.Matches($"{item}");
                    if (matches.Count > 0)
                    {
                        int unprintable_chars = matches.Cast<Match>().Sum(m => m.Length);
                        item_width -= unprintable_chars;
                    }

                    if (item_width > current_width) widths[p.Name] = item_width;
                }
            }
        }

        // 4. Loop through each column to determine it's default alignment by inspecting the type of the first item.
        foreach (string column_name in column_names.Intersect(widths.Keys))
        {
            var first_item = objects
                .Select(obj => GetPropertyValue(obj.Properties[column_name]))
                .FirstOrDefault(v => v != null);

            var right_align_types = FormatColumnSettings.Instance.RightAlignTypes;
            var align_right       = first_item != null && right_align_types.Contains(first_item.GetType());

            if (!align_right) widths[column_name] = -widths[column_name];
        }

        // 5. Overwrite default alignment - Right
        if (align_right_columns != null)
            foreach (string column_name in align_right_columns.Intersect(widths.Keys))
                widths[column_name] = Math.Abs(-widths[column_name]);

        // 6. Overwrite default alignment - Left. Left takes priority over Right.
        if (align_left_columns != null)
            foreach (string column_name in align_left_columns.Intersect(widths.Keys))
                widths[column_name] = -Math.Abs(-widths[column_name]);

        return widths;
    }

    private static void PrintColumns(
        PSCmdlet cmdlet,
        IEnumerable<PSObject> objects,
        IEnumerable<string> column_names,
        IDictionary<string, int> column_widths,
        bool is_indent,
        bool has_row_separator,
        string header_style,
        string border_style,
        string reset_style
    ) {

        string indent = is_indent ? "  " : string.Empty;
        string header = indent + MakeHeader(column_names, column_widths);
        string border = indent + MakeBorder(column_names, column_widths);

        // Print out the header.
        cmdlet.WriteObject($"{header_style}{header}{reset_style}");
        //Console.WriteLine($"{header_style}{header}{reset_style}");

        // Print out the border.
        cmdlet.WriteObject($"{border_style}{border}{reset_style}");
        //Console.WriteLine($"{border_style}{border}{reset_style}");

        // Print out the rows.
        foreach (var obj in objects)
        {
            // Get the number of inner rows.
            int rows_needed = GetRowsNeeded(obj);
            var expanded_objects = ExpandPSObject(obj, rows_needed);

            for (int i = 0; i < expanded_objects.Count(); i++)
            {
                // Print out this inner row.
                string row = indent + MakeRow(expanded_objects.ElementAt(i), column_names, column_widths);
                cmdlet.WriteObject(row);
                //Console.WriteLine(row);

                // Print an empty line in between inner rows. This condition checks if there are more inner rows.
                // if (has_row_separator && i != expanded_objects.Count() - 1) Console.WriteLine();
            }

            // Print separator in between outer rows.
            if (has_row_separator) //Console.WriteLine($"{border_style}{border}{reset_style}");
                cmdlet.WriteObject($"{border_style}{border}{reset_style}");
        }
    }

    private static string MakeHeader(IEnumerable<string> columns, IDictionary<string, int> widths)
    {
        var header = new StringBuilder();

        foreach (var column in columns)
        {
            int width = widths[column];

            if (width == 0) width = column.Length;

            header.AppendFormat($"{{0,{width}}} ", column);
        }

        if (header.Length > 0) header.Length--;

        return header.ToString();
    }

    private static string MakeBorder(IEnumerable<string> columns, IDictionary<string, int> widths)
    {
        var border = new StringBuilder();

        foreach (var column in columns)
        {
            int width = widths[column];

            if (width == 0) width = column.Length;

            string dashes = new('-', Math.Abs(width));

            border.Append(dashes + " ");
        }

        if (border.Length > 0) border.Length--;

        return border.ToString();
    }

    private static string MakeRow(PSObject obj, IEnumerable<string> column_names, IDictionary<string, int> widths)
    {
        var row = new StringBuilder();

        foreach (string column_name in column_names)
        {
            int    column_width = widths[column_name];
            object cell_value   = obj.Properties[column_name]?.Value;

            // Account for unprintable chars (style codes) - add the number to the width.
            var matches = _style_regex.Matches($"{cell_value}");
            if (matches.Count > 0)
            {
                int unprintable_chars = matches.Cast<Match>().Sum( m => m.Length);

                Debug.Assert(column_width != 0);

                column_width = column_width >= 0
                    ? column_width + unprintable_chars
                    : column_width - unprintable_chars;
            }
            row.AppendFormat($"{{0, {column_width}}} ", cell_value);
        }
        return row.ToString();
    }

    // Retrieve the value of a PropertyInfo object, unboxing it if it is a PSObject.
    private static object GetPropertyValue(PSPropertyInfo pi)
    {
        return (pi?.Value as PSObject)?.BaseObject ?? pi?.Value;
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

    // Get the item count of the property with the most items.
    private static int GetRowsNeeded(PSObject obj)
    {
        int rows_needed = 1;

        foreach(PSPropertyInfo p in obj.Properties)
        {
            var property_items_count = GetItems(p.Value).ToList().Count;

            if (property_items_count > rows_needed)
                rows_needed = property_items_count;
        }

        return rows_needed;
    }

    // Flatten the PSObject - Expand depth (collections inside properties) to height (additional rows).
    private static IEnumerable<PSObject> ExpandPSObject(PSObject obj, int count)
    {
        for(int i = 0; i < count; i++)
        {
            var my_object = new PSObject();

            foreach (PSPropertyInfo p in obj.Properties)
            {
                var property_items    = GetItems(p.Value).ToList();
                var my_property_value = i < property_items.Count ? property_items[i] : null;

                my_object.Properties.Add(
                    new PSNoteProperty(p.Name, my_property_value)
                );
            }
            yield return my_object;
        }
    }
}