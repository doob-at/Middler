using doob.Middler.Common;
using doob.Middler.Common.Interfaces;

namespace doob.Middler.Core
{
    public class ActionHelper: IActionHelper
    {
        private IMiddlerRequestContext _middlerRequestContext;

        public ActionHelper(IMiddlerRequestContext middlerRequestContext)
        {
            _middlerRequestContext = middlerRequestContext;
        }


        //public string BuildPathFromRoutData(string template)
        //{

            
        //    var queryObj = new ExpandableObject
        //    {
        //        ["*"] = _middlerRequestContext.Uri.Query.ToNull()?.Substring(1)
        //    };
        //    foreach (var (key, value) in _middlerRequestContext.QueryParameters)
        //    {
        //        queryObj[key] = value;
        //    }

        //    //queryObj.Import(_middlerRequestContext.QueryParameters, renamer: member => member.Name);
        //    //var uriObj = new ExpandableObject(_middlerRequestContext.Uri);
        //    // Json.Converter.ToDictionary(_middlerRequestContext.Uri);


        //    //var routeObj = new ExpandableObject(_middlerRequestContext.RouteData);

        //    var scriptObj = new ScriptObject
        //    {
        //        ["Route"] = _middlerRequestContext.RouteData,
        //        ["Query"] = queryObj,
        //        ["Uri"] = _middlerRequestContext.Uri
        //    };


        //    var scribanTemplate = Template.Parse(template);
        //    var result = scribanTemplate.Render(scriptObj, member => member.Name);

        //    return result;

        //    //string ProcessHtmlTag(Match m)
        //    //{
        //    //    string part = m.Groups["part"].Value;

        //    //    if (part.StartsWith("@"))
        //    //    {
        //    //        part = part.Substring(1);
        //    //        try
        //    //        {
        //    //            return _middlerRequestContext.Uri.GetPropertyValue<object>(part).ToString();
        //    //        }
        //    //        catch
        //    //        {
        //    //            // ignored
        //    //        }
        //    //    }

        //    //    if (part.StartsWith("?"))
        //    //    {
        //    //        part = part.Substring(1);
        //    //        if (part == "*")
        //    //        {
        //    //            return _middlerRequestContext.Uri.Query?.Substring(1);
        //    //        }
        //    //        return _middlerRequestContext.QueryParameters.TryGetValue(part, out var qp) ? qp : null;
        //    //    }

        //    //    return _middlerRequestContext.RouteData.TryGetValue(part, out var rd) ? rd.ToString() : null;

        //    //}

        //    //Regex regex = new Regex("{(?<part>([?@a-zA-Z0-9*]*))}");
        //    //string cleanString = regex.Replace(template, ProcessHtmlTag);
        //    //return cleanString;
        //}
    }


}
