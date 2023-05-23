using System.Collections;
using System.Collections.Generic;
using doob.Middler.Action.ReverseProxy;
using doob.Middler.Common;
using doob.Reflectensions.ExtensionMethods;
using doob.Scripter.Shared;
using Microsoft.AspNetCore.Http;

namespace doob.Middler.Action.Scripting.Models
{
    public class ScriptContextResponse
    {

        public object? ResponseBody { get; set; }
        public int StatusCode => _middlerResponse.Response.StatusCode;

        public Dictionary<string, string> Headers
        {
            get => _middlerResponse.Response.Headers;
            set => _middlerResponse.Response.Headers = value;
        }

        private readonly IMiddlerContext _middlerResponse;
        private readonly IScriptEngine _scriptEngine;

        public ScriptContextResponse(IMiddlerContext middlerResponse, IScriptEngine scriptEngine)
        {
            _middlerResponse = middlerResponse;
            _scriptEngine = scriptEngine;
        }
       

        public void Send(int statusCode, object? body = null)
        {
            _middlerResponse.Response.StatusCode = statusCode;
            ResponseBody = body;
            
            _scriptEngine.Stop();
        }
        
        public void Ok(object? body = null)
        {
            Send(StatusCodes.Status200OK, body);
        }

        public void BadRequest(object? body = null)
        {
            Send(StatusCodes.Status400BadRequest, body);
        }

        public void NotFound(object? body = null)
        {
            Send(StatusCodes.Status404NotFound, body);
        }

        public void Redirect(string location, bool preserveMethod = true)
        {
            Headers["Location"] = location;
            var statusCode = preserveMethod ? StatusCodes.Status307TemporaryRedirect : StatusCodes.Status302Found;
            Send(statusCode);
        }

        public void RedirectPermanent(string location, bool preserveMethod = true)
        {
            Headers["Location"] = location;
            var statusCode = preserveMethod ? StatusCodes.Status308PermanentRedirect : StatusCodes.Status301MovedPermanently;
            Send(statusCode);
        }

        public void Unauthorized(object? body = null)
        {
            Send(StatusCodes.Status401Unauthorized, body);
        }

        public void Forbidden(object? body = null)
        {
            Send(StatusCodes.Status403Forbidden, body);
        }

        public void FromProxyRequest(string url)
        {

            var proxyAction = new ReverseProxyAction();
            proxyAction.Parameters = new ReverseProxyOptions()
            {
                Destination = url
            };
            Send(200, proxyAction);
            
        }

    }
}
