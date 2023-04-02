using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using doob.Middler.Action.Scripting.Helper;
using doob.Middler.Common;
using doob.Reflectensions.ExtensionMethods;
using doob.Scripter.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Forwarder;

namespace doob.Middler.Action.Scripting.Models
{
    public class ScriptContextRequestProxy
    {
        private readonly IMiddlerContext _middlerContext;
        private readonly IScriptEngine _scriptEngine;

        public ScriptContextRequestProxy(IMiddlerContext middlerContext, IScriptEngine scriptEngine)
        {
            _middlerContext = middlerContext;
            _scriptEngine = scriptEngine;
        }

        public void ForwardTo(string destination)
        {
            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                //UseProxy = true,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
                ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
                //Proxy = new WebProxy("http://localhost:8888"),
                SslOptions = new SslClientAuthenticationOptions()
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
                }

            });

            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
            var httpForwarder = _middlerContext.RequestServices.GetRequiredService<IHttpForwarder>();
            var httpContext = _middlerContext.Reflect()
                .GetPropertyValue<HttpContext>("HttpContext", BindingFlags.NonPublic | BindingFlags.Instance);


            var error = httpForwarder
                .SendAsync(httpContext, destination, httpClient, requestOptions, new PathTransformer()).GetAwaiter().GetResult();
           
        }
    }
}
