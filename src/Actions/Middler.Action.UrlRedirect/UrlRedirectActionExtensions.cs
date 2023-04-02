using System;
using doob.Middler.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Action.UrlRedirect
{
    public static class UrlRedirectActionExtensions
    {
        
        public static IMiddlerOptionsBuilder AddUrlRedirectAction(this IMiddlerOptionsBuilder optionsBuilder, string alias = null)
        {
            alias = !String.IsNullOrWhiteSpace(alias) ? alias : UrlRedirectAction.DefaultActionType;
            optionsBuilder.ServiceCollection.AddTransient<UrlRedirectAction>();
            optionsBuilder.RegisterAction<UrlRedirectAction>(alias);

            return optionsBuilder;
        }
    }
}
