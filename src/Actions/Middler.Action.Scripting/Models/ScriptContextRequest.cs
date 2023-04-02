using System;
using System.Linq;
using System.Reflection;
using doob.Middler.Common;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Models;
using doob.Reflectensions;
using doob.Reflectensions.ExtensionMethods;
using doob.Scripter.Shared;
using Microsoft.AspNetCore.Http;

namespace doob.Middler.Action.Scripting.Models
{
    public class ScriptContextRequest
    {
        public string HttpMethod => _middlerRequestContext.HttpMethod;

        public Uri Uri
        {
            get => _middlerRequestContext.Uri;
            set => _middlerRequestContext.Uri = value;
        }
        public MiddlerRouteData RouteData => _middlerRequestContext.RouteData;
        public ExpandableObject Headers => _middlerRequestContext.Headers;
        public MiddlerRouteQueryParameters QueryParameters => _middlerRequestContext.QueryParameters;
        public string User => Authenticated ? (_middlerRequestContext.Principal?.Identity?.Name ?? "Anonymous") : "Anonymous";

        public string? Client => _middlerRequestContext.Principal.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;

        public bool Authenticated => _middlerRequestContext.Principal.Identity?.IsAuthenticated ?? false;

        public string? ClientIp => _middlerRequestContext.SourceIPAddress?.ToString();
        public string[] ProxyServers => _middlerRequestContext.ProxyServers.Select(ip => ip.ToString()).ToArray();

        public HttpRequestBody Body { get; }

        private readonly IMiddlerRequestContext _middlerRequestContext;
        private readonly IScriptEngine _scriptEngine;
        private readonly IMiddlerOptions _middlerOptions;

        public ScriptContextRequest(IMiddlerRequestContext middlerRequestContext, IScriptEngine scriptEngine, IMiddlerOptions middlerOptions)
        {
            _middlerRequestContext = middlerRequestContext;
            _scriptEngine = scriptEngine;
            _middlerOptions = middlerOptions;
            Body = new HttpRequestBody(
                middlerRequestContext.Reflect().GetPropertyValue<HttpContext>("HttpContext", BindingFlags.NonPublic | BindingFlags.Instance)!.Request,
                _scriptEngine,
                _middlerOptions);
        }
        
    }
}
