using System;
using doob.Middler.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Action.UrlRewrite
{
    public static class UrlRewriteActionExtensions
    {
        public static IMiddlerOptionsBuilder AddUrlRewriteAction(this IMiddlerOptionsBuilder optionsBuilder, string alias = null)
        {
            alias = !String.IsNullOrWhiteSpace(alias) ? alias : UrlRewriteAction.DefaultActionType;
            optionsBuilder.ServiceCollection.AddTransient<UrlRewriteAction>();
            optionsBuilder.RegisterAction<UrlRewriteAction>(alias);

            return optionsBuilder;
        }
        
    }
}