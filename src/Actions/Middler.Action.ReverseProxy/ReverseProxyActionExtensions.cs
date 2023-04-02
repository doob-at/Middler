using doob.Middler.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Action.ReverseProxy;

public static class ReverseProxyActionExtensions
{
        
    public static IMiddlerOptionsBuilder AddReverseProxyAction(this IMiddlerOptionsBuilder optionsBuilder, string alias = null)
    {
        optionsBuilder.ServiceCollection.AddHttpForwarder();

        alias = !string.IsNullOrWhiteSpace(alias) ? alias : ReverseProxyAction.DefaultActionType;
        optionsBuilder.ServiceCollection.AddTransient<ReverseProxyAction>();
        optionsBuilder.RegisterAction<ReverseProxyAction>(alias);

        return optionsBuilder;
    }


    public static void AddReverseProxy(this IMiddlerMapActionsBuilder builder, ReverseProxyOptions options)
    {
        builder.AddAction<ReverseProxyAction, ReverseProxyOptions>(options);
    }
}