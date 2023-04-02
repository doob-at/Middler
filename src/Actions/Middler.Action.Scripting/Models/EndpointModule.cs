using System.Collections.Generic;
using doob.Middler.Action.Scripting.Helper;
using doob.Middler.Common;
using doob.Middler.Common.Interfaces;
using doob.Scripter.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Action.Scripting.Models
{
    [ScripterModule("EndpointRule")]
    public class EndpointModule: IScripterModule
    {
       
        public ScriptContextResponse Response { get; }
        public ScriptContextRequest Request { get; }
        //public ScriptContextRequestProxy RequestProxy { get; }
        public EndpointHelper Helper => new EndpointHelper();
        public Dictionary<string, object> PropertyBag => _middlerContext.PropertyBag;

        public EndpointOptions Options { get; set; } = new EndpointOptions();

        private readonly IMiddlerContext _middlerContext;
        private readonly IScriptEngine _scriptEngine;
        public EndpointModule(EndpointModuleInitializeParameters endpointModuleInitializeParameters)
        {
            
            _middlerContext = endpointModuleInitializeParameters.MiddlerContext;
            Options.Terminating = endpointModuleInitializeParameters.Terminating;
            _scriptEngine = endpointModuleInitializeParameters.ScriptEngine;
            Request = new ScriptContextRequest(_middlerContext.Request, _scriptEngine, _middlerContext.RequestServices.GetRequiredService<IMiddlerOptions>());
            Response = new ScriptContextResponse(_middlerContext, _scriptEngine);
            //RequestProxy = new ScriptContextRequestProxy(_middlerContext, _scriptEngine);
        }


        
     
    }

    public class EndpointOptions
    {
        private bool _terminating;

        public bool Terminating
        {
            get => _terminating;
            set => _terminating = value;
        }
    }

    public record EndpointModuleInitializeParameters(
        IMiddlerContext MiddlerContext, 
        IScriptEngine ScriptEngine, 
        bool Terminating);

    public class EndpointHelper
    {
        private readonly MimeTypeMappingService mimeTypeMappingService = new();

        public string GetMimeType(string fileName)
        {
            return mimeTypeMappingService.GetMimeType(fileName);
        }
    }
}
