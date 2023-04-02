using System;
using System.Collections.Generic;
using System.Linq;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Interfaces;
using doob.Middler.Common.SharedModels.Models;
using doob.Reflectensions.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Core
{
    public class InternalHelper
    {
        public IServiceProvider ServiceProvider { get; }
        public IMiddlerOptions MiddlerOptions { get; }

        public InternalHelper(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            MiddlerOptions = serviceProvider.GetRequiredService<IMiddlerOptions>();
        }

        public IMiddlerAction? BuildBasicActionInstanceFromTypeName(string actionType)
        {
            if (String.IsNullOrWhiteSpace(actionType))
                return null;

            var actType = GetConcreteActionType(actionType);
            if (actType == null)
                return null;

            return (IMiddlerAction)ActivatorUtilities.CreateInstance(ServiceProvider, actType);
            
        }

        public IMiddlerAction? BuildConcreteActionInstance(MiddlerAction middlerAction)
        {
            if (middlerAction.ActionType == null)
                return null;

            var actType = GetConcreteActionType(middlerAction.ActionType);
            if (actType == null)
                return null;

            var concreteAction = (IMiddlerAction)ActivatorUtilities.CreateInstance(ServiceProvider, actType);
            var hasParameters = actType.BaseType?.GenericTypeArguments.Any() == true;

            if (hasParameters)
            {
                var genT = actType.BaseType!.GenericTypeArguments[0];
                var actParams = Converter.Json.ToObject(Converter.Json.ToJson(middlerAction.Parameters), genT);
                concreteAction.Reflect().SetPropertyValue("Parameters", actParams!);
            }

            concreteAction.Terminating = middlerAction.Terminating;
            concreteAction.WriteStreamDirect = middlerAction.WriteStreamDirect;

            return concreteAction;

        }

        public Type? GetConcreteActionType(string actionType)
        {
            return MiddlerOptions.RegisteredActionTypes.TryGetValue(actionType, out var actType) ? actType : null;
        }
        
        public string? GetRegisteredActionTypeAlias<T>()
        {
            return GetRegisteredActionTypeAlias(typeof(T));
        }
        public string? GetRegisteredActionTypeAlias(Type type)
        {
            var regType = MiddlerOptions.RegisteredActionTypes.FirstOrDefault(kv => kv.Value == type);
            if (regType.Equals(default(KeyValuePair<string, Type>)))
            {
                return null;
            }

            return regType.Key;
        }

        public Type? GetRegisteredActionType(string alias)
        {
            return !MiddlerOptions.RegisteredActionTypes.TryGetValue(alias, out var actType) ? null : actType;
        }

        public MiddlerAction ConvertToBasicMiddlerAction<T>(MiddlerAction<T> middlerAction) where T : class, new()
        {

            var typeAlias = GetRegisteredActionTypeAlias(middlerAction.GetType());

            if (typeAlias == null)
                throw new TypeAccessException(middlerAction.GetType().FullName);

            var act = new MiddlerAction
            {
                Terminating = middlerAction.Terminating,
                WriteStreamDirect = middlerAction.WriteStreamDirect,
                ActionType = typeAlias,
                Parameters = Converter.Json.ToJObject(middlerAction.Parameters)
                    //middlerAction.Parameters?.GetType().GetProperties()
                    //    .ToDictionary(p => p.Name, p => p.GetValue(middlerAction.Parameters)) ??
                    //new Dictionary<string, object>()
            };

            return act;
        }
    }
}
