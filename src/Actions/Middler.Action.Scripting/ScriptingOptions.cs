using doob.Middler.Common.SharedModels.Attributes;

namespace doob.Middler.Action.Scripting
{
    public class ScriptingOptions 
    {
        public string? Language { get; set; }

        public string? SourceCode { get; set; }

        [Internal]
        public string? CompiledCode { get; set; }
    }
}
