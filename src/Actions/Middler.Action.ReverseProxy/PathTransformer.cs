using System.Net.Http;
using System.Threading.Tasks;
using doob.Middler.Common;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;

namespace doob.Middler.Action.ReverseProxy;


internal class PathTransformer : HttpTransformer
{
    private readonly IMiddlerContext _middlerContext;
    private readonly ReverseProxyOptions _options;

    public PathTransformer(IMiddlerContext middlerContext, ReverseProxyOptions options)
    {
        _middlerContext = middlerContext;
        _options = options;
    }
    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
        proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, "", httpContext.Request.QueryString);
        proxyRequest.Headers.Host = null;
       
    }
}
