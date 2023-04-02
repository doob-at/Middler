using System;
using System.Collections.Generic;
using doob.Middler.Common;
using doob.Middler.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace doob.Middler.Core.Context
{
    public class MiddlerContext: IMiddlerContext 
    {
        internal MiddlerRequestContext MiddlerRequestContext { get; }
        public IMiddlerRequestContext Request => MiddlerRequestContext;

        internal MiddlerResponseContext MiddlerResponseContext { get; }
        public IMiddlerResponseContext Response => MiddlerResponseContext;
        public Dictionary<string, object> PropertyBag { get; } = new ();

        public IFeatureCollection Features => HttpContext.Features;
        public IServiceProvider RequestServices => HttpContext.RequestServices;
        //public void ForwardBody()
        //{
        //    MiddlerResponseContext.StreamBody = MiddlerRequestContext.Body;
        //}


        private HttpContext HttpContext { get; }
        private IMiddlerOptions MiddlerOptions { get; }

        
        //private FakeHttpContext HttpContext { get; }
        public MiddlerContext(HttpContext httpContext, IMiddlerOptions middlerOptions)
        {
            //_oldStream = httpContext.Response.Body;
            //httpContext.Response.Body = new AutoStream(opts => 
            //    opts
            //        .WithMemoryThreshold(middlerOptions.AutoStreamDefaultMemoryThreshold)
            //        .WithFilePrefix("middler"), Request.RequestAborted);
            HttpContext = httpContext;
            MiddlerOptions = middlerOptions;
            MiddlerRequestContext = new MiddlerRequestContext(httpContext, middlerOptions);
            MiddlerResponseContext = new MiddlerResponseContext(httpContext, middlerOptions);
            
            //MiddlerResponseContext.StreamBody = new AutoStream(opts => 
            //    opts
            //        .WithMemoryThreshold(middlerOptions.AutoStreamDefaultMemoryThreshold)
            //        .WithFilePrefix("middler"), Request.RequestAborted);

            //HttpContext = new FakeHttpContext(this, httpContext);
            
        }


        public void SetRouteData(IDictionary<string, IRouteConstraint> constraints)
        {
            MiddlerRequestContext.SetRouteData(constraints);
        }


        //public void PrepareNext()
        //{

            
        //    //MiddlerRequestContext.SetNextBody(MiddlerResponseContext.Body);
        //    if (MiddlerRequestContext.Body.CanSeek)
        //    {
        //        MiddlerRequestContext.Body.Seek(0, SeekOrigin.Begin);
        //    }
           

        //    MiddlerResponseContext.Body = new AutoStream(opts => 
        //        opts
        //            .WithMemoryThreshold(MiddlerOptions.AutoStreamDefaultMemoryThreshold)
        //            .WithFilePrefix("middler"), Request.RequestAborted);

        //}
    }
}
