using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using doob.Middler.Common;
using doob.Middler.Common.ExtensionMethods;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Enums;
using doob.Middler.Common.SharedModels.Interfaces;
using doob.Middler.Common.SharedModels.Models;
using doob.Middler.Common.StreamHelper;
using doob.Middler.Core.Context;
using doob.Middler.Core.ExtensionMethods;
using doob.Middler.Core.Helper;
using doob.Middler.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Type = System.Type;

namespace doob.Middler.Core
{
    public class MiddlerMiddleware
    {

        private readonly RequestDelegate _next;
        private ILogger? Logger { get; set; }
        private ILogger? ConstraintLogger { get; set; }
        
        public MiddlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IMiddlerOptions middlerOptions, InternalHelper intHelper)
        {
            var sw = new Stopwatch();

            sw.Start();
            EnsureLoggers(httpContext);
            //Stream originalBody = null;
            
            var originalStream = httpContext.Response.Body;
            var aStream = new AutoStream(opts =>
                opts
                    .WithMemoryThreshold(middlerOptions.AutoStreamDefaultMemoryThreshold)
                    .WithTempDirectory(middlerOptions.TemporaryFilePath)
                    .WithFilePrefix("middler"), httpContext.RequestAborted);
            httpContext.Response.Body = aStream;


            var middlerMap = httpContext.RequestServices.GetRequiredService<IMiddlerMap>();


            var endpoints = middlerMap.GetFlatList(httpContext.RequestServices);

            var executedActions = new Stack<IMiddlerAction>();

            var middlerContext = new MiddlerContext(httpContext, middlerOptions);

            try
            {
                bool terminating;
                do
                {

                    var matchingEndpoint = FindMatchingEndpoint(middlerOptions, endpoints, middlerContext);

                    if (matchingEndpoint == null)
                    {
                        break;
                    }

                    if (matchingEndpoint.AccessMode == AccessMode.Deny)
                    {
                        await httpContext.Forbid().ConfigureAwait(false);
                        return;
                    }

                    endpoints = matchingEndpoint.RemainingEndpointInfos;

                    terminating = false;
                    foreach (var endpointAction in matchingEndpoint.MiddlerRule.Actions)
                    {
                        if (!endpointAction.Enabled)
                        {
                            continue;
                        }

                        var action = intHelper.BuildConcreteActionInstance(endpointAction);
                        if (action != null)
                        {

                            await ExecuteRequestAction(action, middlerContext);
                            executedActions.Push(action);
                            terminating = action.Terminating;
                        }

                        if (terminating)
                            break;
                    }


                } while (!terminating);


                if (executedActions.Any())
                {
                    await WriteToAspNetCoreResponseBodyAsync(httpContext, middlerContext, originalStream).ConfigureAwait(false);
                }
                else
                {
                    await _next(httpContext);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }



        }


        private async Task ExecuteRequestAction(IMiddlerAction middlerAction, MiddlerContext middlerContext)
        {
            var method = middlerAction.GetType().GetMethod("ExecuteRequestAsync") ?? middlerAction.GetType().GetMethod("ExecuteRequest");
            if (method == null)
            {
                return;
            }

            var man = new Dictionary<Type, object>()
            {

            };



            var parameters = BuildExecuteMethodParameters(method, middlerContext, man);

            if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                var t = (Task)method.Invoke(middlerAction, parameters)!;
                await t.ConfigureAwait(false);
            }
            else
            {
                method.Invoke(middlerAction, parameters);
            }

        }

        private async Task ExecuteResponseAction(IMiddlerAction middlerAction, MiddlerContext middlerContext)
        {
            var method = middlerAction.GetType().GetMethod("ExecuteResponseAsync") ?? middlerAction.GetType().GetMethod("ExecuteResponse");
            if (method == null)
            {
                return;
            }

            var man = new Dictionary<Type, object>
            {

            };

            var parameters = BuildExecuteMethodParameters(method, middlerContext, man);

            if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                var t = (Task)method.Invoke(middlerAction, parameters)!;
                await t.ConfigureAwait(false);
            }
            else
            {
                method.Invoke(middlerAction, parameters);
            }

        }


        private object[] BuildExecuteMethodParameters(MethodInfo methodInfo, MiddlerContext middlerContext, Dictionary<Type, object> manualCreateParameters)
        {

            return methodInfo.GetParameters().Select(p =>
            {

                if (manualCreateParameters.TryGetValue(p.ParameterType, out var manualCreated))
                    return manualCreated;

                if (p.ParameterType == typeof(IMiddlerContext))
                {
                    return middlerContext;
                }


                if (p.ParameterType == typeof(IActionHelper))
                {
                    return new ActionHelper(middlerContext.Request);
                }

                return middlerContext.RequestServices.GetRequiredService(p.ParameterType);

            }).ToArray();

        }


        private async Task WriteToAspNetCoreResponseBodyAsync(HttpContext context, MiddlerContext middlerContext, Stream originalStream)
        {

            if (!context.Response.HasStarted)
            {
                foreach (var (key, value) in middlerContext.MiddlerResponseContext.Headers)
                {
                    context.Response.Headers[key] = value;
                }

                if (middlerContext.Response.StatusCode != 0)
                {
                    context.Response.StatusCode = middlerContext.Response.StatusCode;
                }
            }

            var middlerStream = context.Response.Body;
            context.Response.Body = originalStream;

            if (context.Response.Body.CanWrite)
            {
                if(middlerStream.CanSeek)
                    middlerStream.Seek(0, SeekOrigin.Begin);

                try
                {
                    await middlerStream.CopyToAsync(context.Response.Body, 81920, context.RequestAborted);
                }
                catch (TaskCanceledException e)
                {
                    //ignore
                }

            }

            await middlerStream.DisposeAsync().ConfigureAwait(false);
        }


        private MiddlerRuleMatch? FindMatchingEndpoint(IMiddlerOptions middlerOptions, List<MiddlerRule> rule, MiddlerContext middlerContext)
        {

            for (int i = 0; i < rule.Count; i++)
            {
                var match = CheckMatch(middlerOptions, rule[i], middlerContext);
                if (match != null)
                {

                    if (match.AccessMode != AccessMode.Ignore)
                    {
                        match.RemainingEndpointInfos = rule.Skip(i + 1).ToList();
                        return match;
                    }
                }

            }

            return null;
        }


        private MiddlerRuleMatch? CheckMatch(IMiddlerOptions middlerOptions, MiddlerRule rule, MiddlerContext middlerContext)
        {

            var allowedHttpMethods = (rule.HttpMethods?.IgnoreNullOrWhiteSpace().Any() == true ? rule.HttpMethods.IgnoreNullOrWhiteSpace() : middlerOptions.DefaultHttpMethods).ToList();

            if (allowedHttpMethods.Any() && !allowedHttpMethods.Contains(middlerContext.Request.HttpMethod, StringComparer.OrdinalIgnoreCase))
                return null;

            //var uri = new Uri(context.Request.GetEncodedUrl());

            var allowedSchemes = (rule.Scheme?.IgnoreNullOrWhiteSpace().Any() == true ? rule.Scheme.IgnoreNullOrWhiteSpace() : middlerOptions.DefaultScheme).ToList();

            if (allowedSchemes.Any() && !allowedSchemes.Any(scheme => Wildcard.Match(middlerContext.MiddlerRequestContext.Uri.Scheme, scheme)))
                return null;

            if (rule.Hostname != null)
            {
                var matchHostName = rule.Hostname.Split(';').Select(s => s.Trim()).Any(hName => Wildcard.Match(middlerContext.MiddlerRequestContext.Uri.Host, hName));
                if (!matchHostName)
                {
                    return null;
                }
            }
            

            if (String.IsNullOrWhiteSpace(rule.Path))
            {
                rule.Path = "{**path}";
            }

            var parsedTemplate = TemplateParser.Parse(rule.Path);

            var defaults = parsedTemplate.Parameters.Where(p => p.DefaultValue != null)
                .Aggregate(new RouteValueDictionary(), (current, next) =>
                {
                    current.Add(next.Name!, next.DefaultValue);
                    return current;
                });

            var matcher = new TemplateMatcher(parsedTemplate, defaults);
            var rd = middlerContext.MiddlerRequestContext.GetRouteData();
            var router = rd.Routers.FirstOrDefault() ?? new RouteCollection();

            if (matcher.TryMatch(middlerContext.MiddlerRequestContext.Uri.AbsolutePath, rd.Values))
            {
                var constraints = GetConstraints(middlerContext.RequestServices.GetRequiredService<IInlineConstraintResolver>(), parsedTemplate, null);
                if (MiddlerRouteConstraintMatcher.Match(constraints, rd.Values, router, RouteDirection.IncomingRequest, ConstraintLogger!))
                {

                    middlerContext.SetRouteData(constraints);
                    var accessMode = rule.AccessAllowed(middlerContext.Request) ?? middlerOptions.DefaultAccessMode;
                    return new MiddlerRuleMatch(rule, accessMode);

                }
            }

            return null;
        }


        private static IDictionary<string, IRouteConstraint> GetConstraints(IInlineConstraintResolver inlineConstraintResolver, RouteTemplate parsedTemplate, IDictionary<string, object>? constraints)
        {

            var constraintBuilder = new RouteConstraintBuilder(inlineConstraintResolver, parsedTemplate.TemplateText!);

            if (constraints != null)
            {
                foreach (var kvp in constraints)
                {
                    constraintBuilder.AddConstraint(kvp.Key, kvp.Value);
                }
            }

            foreach (var parameter in parsedTemplate.Parameters)
            {

                if (parameter.IsOptional)
                {
                    constraintBuilder.SetOptional(parameter.Name!);
                }

                foreach (var inlineConstraint in parameter.InlineConstraints)
                {
                    constraintBuilder.AddResolvedConstraint(parameter.Name!, inlineConstraint.Constraint);
                }

            }

            return constraintBuilder.Build();
        }

        private void EnsureLoggers(HttpContext context)
        {

            if (Logger == null)
            {
                var factory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                Logger = factory.CreateLogger(typeof(RouteBase).FullName);
                ConstraintLogger = factory.CreateLogger(typeof(RouteConstraintMatcher).FullName);
            }

        }


    }
}
