﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using doob.Reflectensions.Common;
using Microsoft.AspNetCore.Http;

namespace doob.Middler.Core.ExtensionMethods
{
    internal static class HttpRequestExtensions
    {
        public static List<IPAddress> FindSourceIp(this HttpRequest httpRequest)
        {


            var sourceIps = new List<IPAddress>();


            if (httpRequest.Headers.ContainsKey("X-Forwarded-For"))
            {
                var ips = httpRequest.Headers["X-Forwarded-For"];
                sourceIps = ips.Select(IPAddress.Parse).ToList();
            }

            if(httpRequest.HttpContext.Connection.RemoteIpAddress != null)
                sourceIps.Add(httpRequest.HttpContext.Connection.RemoteIpAddress);

            return sourceIps;
        }

        internal static Dictionary<string, string> GetHeaders(this HttpRequest request) => request.Headers.ToDictionary(kvp => kvp.Key, kvp => String.Join(", ", kvp.Value));

        internal static Dictionary<string, string> GetHeaders(this HttpResponse request) => request.Headers.ToDictionary(kvp => kvp.Key, kvp => String.Join(", ", kvp.Value));


        internal static Dictionary<string, string?> GetQueryParameters(this HttpRequest request)
        {

            var dictionary = new Dictionary<string, string?>();

            foreach (var pair in request.Query)
            {

                if (String.IsNullOrWhiteSpace(pair.Value))
                {
                    dictionary.Add(pair.Key, "true");
                    continue;
                }
                var value = GetJoinedValue(pair.Value);
                if (value == "")
                    value = null;

                if (value == "\"\"")
                    value = "";

                dictionary.Add(pair.Key, value);
            }

            return dictionary;

        }

        private static string? GetJoinedValue(string[]? value)
        {
            if (value != null)
                return string.Join(",", value.Select(v => v.ToNull() ?? "true"));

            return null;
        }
    }
}
