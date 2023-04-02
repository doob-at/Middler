using System;
using doob.Middler.Common;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Models;
using Microsoft.AspNetCore.Http;
using Scriban;

namespace doob.Middler.Action.UrlRedirect
{
    public class UrlRedirectAction: MiddlerAction<UrlRedirectOptions>
    {
        internal static string DefaultActionType => "UrlRedirect";
        public override bool Terminating => true;

        public void ExecuteRequest(IMiddlerContext middlerContext, IActionHelper actionHelper)
        {
            var template = Template.Parse(Parameters.RedirectTo);
            var redirectTo = template.Render(middlerContext.Request);
            var isAbsolute = Uri.IsWellFormedUriString(redirectTo, UriKind.Absolute);
            Uri uri;
            if (isAbsolute)
            {
                uri = new Uri(redirectTo);
            }
            else
            {
                var builder = new UriBuilder(middlerContext.Request.Uri);
                builder.Query = null;
                if (redirectTo.Contains("?"))
                {
                    builder.Path = redirectTo.Split("?")[0];
                    builder.Query = redirectTo.Split("?")[1];
                }
                else
                {
                    builder.Path = redirectTo;
                }


                uri = builder.Uri;
            }

            if (Parameters.PreserveMethod)
            {
                middlerContext.Response.StatusCode = Parameters.Permanent ? StatusCodes.Status308PermanentRedirect : StatusCodes.Status307TemporaryRedirect;
            }
            else
            {
                middlerContext.Response.StatusCode =  Parameters.Permanent ? StatusCodes.Status301MovedPermanently : StatusCodes.Status302Found;
            }

            middlerContext.Response.Headers["Location"] = uri.AbsoluteUri;
           
        }

        public override string ActionType => DefaultActionType;

       
    }
}
