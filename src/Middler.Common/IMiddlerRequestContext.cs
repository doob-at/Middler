using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using doob.Middler.Common.SharedModels.Models;
using doob.Reflectensions;

namespace doob.Middler.Common
{
    public interface IMiddlerRequestContext
    {
        
        ClaimsPrincipal Principal { get; set; }
        IPAddress? SourceIPAddress { get; }
        List<IPAddress> ProxyServers { get; }
        MiddlerRouteData RouteData { get; }
        ExpandableObject Headers { get; set; }
        MiddlerRouteQueryParameters QueryParameters { get; set; }
        Uri Uri { get; set; }
        
        string HttpMethod { get; set; }
        CancellationToken RequestAborted { get; }

        string ContentType { get; set; }

        string GetBodyAsString();

        void SetBody(object body);
    }
}
