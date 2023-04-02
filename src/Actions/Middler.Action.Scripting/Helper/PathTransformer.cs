using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;

namespace doob.Middler.Action.Scripting.Helper;


internal class PathTransformer : HttpTransformer
{
    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, "", httpContext.Request.QueryString);
        proxyRequest.Headers.Host = null;
       
    }
}
