using System.Management.Automation;

namespace Worker369.Utility
{
    [Cmdlet(VerbsCommon.Split, "String")]
    [OutputType(typeof(string[]))]
    [Alias("str_split")]
    public class String_Split : PSCmdlet
    {
        [Parameter(Position = 1, ValueFromPipeline = true)]
        [AllowEmptyString, AllowNull]
        public string Input {get; set;}

        [Parameter(Position = 0)]
        [AllowEmptyString, AllowNull]
        public string Delimiter {get; set;}

        protected override void ProcessRecord()
        {
            if (Input is null) return;
            if (Delimiter is null)
            {
                WriteObject(Input);
                return;
            }
            WriteObject(Input.Split([Delimiter], System.StringSplitOptions.None), true);
        }
    }
}