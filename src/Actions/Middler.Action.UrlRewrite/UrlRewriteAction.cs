using System;
using System.Collections.Generic;
using System.Linq;
using doob.Middler.Common;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Models;
using doob.Reflectensions;
using Newtonsoft.Json.Linq;
using Scriban.Runtime;

namespace doob.Middler.Action.UrlRewrite
{
    public class UrlRewriteAction: MiddlerAction<UrlRewriteOptions>
    {
        internal static string DefaultActionType => "UrlRewrite";

        public override bool Terminating => false;

        public override bool WriteStreamDirect => false;
        public override string ActionType => DefaultActionType;


        public void ExecuteRequest(IMiddlerContext middlerContext, IActionHelper actionHelper)
        {

            var rewriteTo = Parse(Parameters.RewriteTo, middlerContext.Request);

            var isAbsolute = Uri.IsWellFormedUriString(rewriteTo, UriKind.Absolute);
            if (isAbsolute)
            {
                middlerContext.Request.Uri = new Uri(rewriteTo);
            }
            else
            {
                var builder = new UriBuilder(middlerContext.Request.Uri);
                builder.Query = null;
                if (rewriteTo.Contains("?"))
                {
                    builder.Path = rewriteTo.Split("?")[0];
                    builder.Query = rewriteTo.Split("?")[1];
                }
                else
                {
                    builder.Path = rewriteTo;
                }
                
                
                middlerContext.Request.Uri = builder.Uri;
            }


        }


        public string Parse(string template, object data)
        {
            return Parse(template, new List<object> {data});
        }

        public string Parse(string template, params object[] data)
        {
            return Parse(template, data.ToList());
        }


        private string Parse(string template, IEnumerable<object> data)
        {
            JObject jobject = new JObject();
            data.Aggregate(jobject, (a, b) => {
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
