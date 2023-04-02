using System;
using doob.Middler.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Action.Scripting
{
    public static class ScriptingActionExtensions
    {
        
        public static IMiddlerOptionsBuilder AddScriptingAction(this IMiddlerOptionsBuilder optionsBuilder, string? alias = null)
        {
            return AddNamedScriptingAction(optionsBuilder, null, alias);
        }

        public static IMiddlerOptionsBuilder AddNamedScriptingAction(this IMiddlerOptionsBuilder optionsBuilder, string? name, string? alias = null)
        {
            alias = !String.IsNullOrWhiteSpace(alias) ? alias : ScriptingAction.DefaultActionType;
            optionsBuilder.ServiceCollection.AddTransient<ScriptingAction>();
            optionsBuilder.RegisterAction<ScriptingAction>(alias);
            var opts = new ScriptingActionOptions
            {
                NamedScripter = name
            };
            optionsBuilder.ServiceCollection.AddSingleton(opts);

            return optionsBuilder;
        }
    }
}
