using System;
using System.Collections.Generic;
using doob.Middler.Common.SharedModels.Models;

namespace doob.Middler.Common.Interfaces {

    public interface IMiddlerMapActionsBuilder {

        IServiceProvider ServiceProvider { get; }
        List<MiddlerAction> MiddlerActions { get; }

        IMiddlerMapActionsBuilder AddAction(MiddlerAction action);
        IMiddlerMapActionsBuilder AddAction<T>() where T : MiddlerAction;
        IMiddlerMapActionsBuilder AddAction<T, TParam>(TParam parameters) where T : MiddlerAction<TParam> where TParam : class, new();
        IMiddlerMapActionsBuilder AddAction<T, TParam>(Action<TParam> parameters) where T : MiddlerAction<TParam> where TParam : class, new();

    }

}
