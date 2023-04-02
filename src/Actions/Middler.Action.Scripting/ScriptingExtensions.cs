using doob.Middler.Common.Interfaces;

namespace doob.Middler.Action.Scripting
{
    public static class ScriptingExtensions
    {

        public static void InvokeScript(this IMiddlerMapActionsBuilder builder, ScriptingOptions options)
        {
            builder.AddAction<ScriptingAction, ScriptingOptions>(options);
        }
        

    }
}