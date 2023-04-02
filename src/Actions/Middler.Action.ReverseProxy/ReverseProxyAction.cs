using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Threading.Tasks;
using doob.Middler.Common;
using doob.Middler.Common.SharedModels.Models;
using doob.Reflectensions;
using doob.Reflectensions.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Scriban.Runtime;
using Yarp.ReverseProxy.Forwarder;

namespace doob.Middler.Action.ReverseProxy
{
    public class ReverseProxyAction : MiddlerAction<ReverseProxyOptions>
    {
        internal static string DefaultActionType => "ReverseProxy";

        public override bool Terminating { get; set; } = true;

        public override bool WriteStreamDirect => false;
        public override string ActionType => DefaultActionType;

        public async Task ExecuteRequestAsync(IMiddlerContext middlerContext)
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



            var parserContext = new Dictionary<string, object>();
            parserContext["RouteData"] = middlerContext.Request.RouteData;
            parserContext["Uri"] = middlerContext.Request.Uri;
            parserContext["Bag"] = middlerContext.PropertyBag;
            var dest = Parse(Parameters.Destination, parserContext);

            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
            var httpForwarder = middlerContext.RequestServices.GetRequiredService<IHttpForwarder>();
            var httpContext = middlerContext.Reflect()
                .GetPropertyValue<HttpContext>("HttpContext", BindingFlags.NonPublic | BindingFlags.Instance);

            var error = await httpForwarder
                .SendAsync(httpContext, dest, httpClient, requestOptions, new PathTransformer(middlerContext, Parameters));
        }

        public string Parse(string template, object data)
        {
            return Parse(template, new List<object> { data });
        }

        public string Parse(string template, params object[] data)
        {
            return Parse(template, data.ToList());
        }


        private string Parse(string template, IEnumerable<object> data)
        {
            JObject jobject = new JObject();
            data.Aggregate(jobject, (a, b) =>
            {
                var json = Json.Converter.ToJson(b);
                var jo = Json.Converter.ToJObject(json);
                return Json.Converter.Merge(a, jo);
            });

            var dict = Json.Converter.ToDictionary(jobject);

            return ParseScriptObject(template, dict!);
        }

        private string ParseScriptObject(string template, Dictionary<string, object> data)
        {
            var scriptObj = new ScriptObject(StringComparer.OrdinalIgnoreCase);
            scriptObj.Import(data, renamer: member => member.Name);
            var scribanTemplate = Scriban.Template.Parse(template);
            return scribanTemplate.Render(scriptObj);
        }
    }
}
