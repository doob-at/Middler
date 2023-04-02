using System;
using System.Threading.Tasks;
using doob.Middler.Action.Scripting.Models;
using doob.Middler.Common;
using doob.Middler.Common.SharedModels.Models;
using doob.Scripter.Shared;
using Microsoft.Extensions.DependencyInjection;
using NamedServices.Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Action.Scripting
{
    public class ScriptingAction: MiddlerAction<ScriptingOptions>
    {
        internal static string DefaultActionType => "Script";
        public override string ActionType => DefaultActionType;

        public override bool Terminating { get; set; } = true;

        private readonly IServiceProvider _serviceProvider;

        private IScriptEngine scriptEngine { get; set; }
        public ScriptingAction(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteRequestAsync(IMiddlerContext middlerContext, ScriptingActionOptions actionOptions)
        {
            
            if (String.IsNullOrWhiteSpace(actionOptions?.NamedScripter))
            {
                scriptEngine = _serviceProvider.GetRequiredService<IScripter>().GetScriptEngine(Parameters.Language);
            }
            else
            {
                scriptEngine = _serviceProvider.GetRequiredNamedService<IScripter>(actionOptions.NamedScripter).GetScriptEngine(Parameters.Language);
            }
            

            var compile = scriptEngine.NeedsCompiledScript && (!string.IsNullOrEmpty(Parameters.SourceCode) &&
                                                               string.IsNullOrWhiteSpace(Parameters.CompiledCode));

            
            if (compile)
            {
                CompileScriptIfNeeded();
            }

            scriptEngine.AddModuleParameterInstance(
                typeof(EndpointModuleInitializeParameters), 
                () => new EndpointModuleInitializeParameters(middlerContext, scriptEngine, Terminating)
                );

            scriptEngine.AddTaggedModules("EndpointRule");
            
            await scriptEngine.ExecuteAsync(scriptEngine.NeedsCompiledScript ? Parameters.CompiledCode! : Parameters.SourceCode!);

            var endpointModule = scriptEngine.GetModuleState<EndpointModule>();
            Terminating = endpointModule?.Options.Terminating ?? Terminating;
        }


        //public void ExecuteResponseAsync()
        //{
        //    var responseFunction = scriptEngine.GetFunction("ExecuteResponse");
        //    if (responseFunction != null)
        //    {
        //        responseFunction.Invoke();
        //    }
        //}

        public string? CompileScriptIfNeeded()
        {
            
            IScriptEngine scriptEngine = _serviceProvider.GetRequiredNamedService<IScriptEngine>(Parameters.Language);
            if (scriptEngine.NeedsCompiledScript)
            {
                Parameters.CompiledCode = scriptEngine.CompileScript.Invoke(Parameters.SourceCode ?? "");
            }

            return Parameters.CompiledCode;
        }


    }
}
