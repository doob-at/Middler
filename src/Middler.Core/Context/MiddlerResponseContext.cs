using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using doob.Middler.Common;
using doob.Middler.Common.ExtensionMethods;
using doob.Middler.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace doob.Middler.Core.Context
{
    public class MiddlerResponseContext : IMiddlerResponseContext
    {
        private HttpContext _httpContext { get; set; }
        //private Stream? _tempStream;

        public int StatusCode
        {
            get => _httpContext.Response.StatusCode;
            set => _httpContext.Response.StatusCode = value;
        }
        public Dictionary<string, string> Headers { get; set; } = new ();

        public MiddlerResponseContext(HttpContext httpContext, IMiddlerOptions middlerOptions)
        {
            _httpContext = httpContext;
        }
       
        //public void SetBody(object? body)
        //{

        //    if (!_httpContext.Response.HasStarted) {
        //        foreach (var (key, value) in Headers) {
        //            _httpContext.Response.Headers[key] = value;
        //        }

        //        if (StatusCode != 0) {
        //            _httpContext.Response.StatusCode = StatusCode;
        //        }
        //    }

        //    if (body == null)
        //    {
        //        return;
        //    }

        //    //if (body is Stream stream)
        //    //{
        //    //    this._tempStream = _httpContext.Response.Body;
        //    //    _httpContext.Response.Body = stream;
        //    //    return;
        //    //}


        //    //if (_tempStream != null)
        //    //{
        //    //    _httpContext.Response.Body = _tempStream;
        //    //}

        //    //_httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        //    //_httpContext.Response.Body.SetLength(0);
            
        //    switch (body)
        //    {
        //        case string str:
        //        {
        //            _httpContext.Content(str).GetAwaiter().GetResult();
        //            return;
        //        }
        //        case PhysicalFileResult pfr:
        //        {
        //            _httpContext.WriteActionResult(pfr).GetAwaiter().GetResult();
        //            break;
        //        }
        //        case Stream str: {
        //            str.CopyToAsync(_httpContext.Response.Body).GetAwaiter().GetResult();
        //            break;
        //        }
        //        default:
        //        {
        //            _httpContext.WriteActionResult(new ObjectResult(body)).GetAwaiter().GetResult();
        //            break;
        //            }
        //    }

        //}

        public Task SetBodyAsync(object? body) {
            if (!_httpContext.Response.HasStarted) {
                foreach (var (key, value) in Headers) {
                    _httpContext.Response.Headers[key] = value;
                }

                if (StatusCode != 0) {
                    _httpContext.Response.StatusCode = StatusCode;
                }
            }

            if (body == null)
            {
                return Task.CompletedTask;
            }

            switch (body)
            {
                
                case string str:
                {
                    return _httpContext.Content(str);
                }
                case PhysicalFileResult pfr:
                {
                    return _httpContext.WriteActionResult(pfr);
                }
                case Stream str: {
                    return str.CopyToAsync(_httpContext.Response.Body);
                }
                default:
                {
                    return _httpContext.WriteActionResult(new ObjectResult(body));
                }
            }
        }
    }
}
