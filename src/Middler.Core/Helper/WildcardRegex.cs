using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace doob.Middler.Core.Helper
{

    public static class Wildcard {

        public static List<string> WildcardCharacters = new List<string> { "*", "?", "+" };


        public static string WildcardToRegex(string pattern) {
            return "^" + Regex.Escape(pattern).
                       Replace("\\*", ".*").
                       Replace("\\?", ".").
                       Replace("\\+", ".+") + "$";
        }

        public static Boolean Match(string? searchIn, String matchString, bool ignoreCase = true, bool invert = false) {
            if (searchIn == null)
                searchIn = "";

            RegexOptions regOpts = RegexOptions.None;
            if (ignoreCase)
                regOpts = regOpts | RegexOptions.IgnoreCase;

            var rege = new Regex(WildcardToRegex(matchString), regOpts).IsMatch(searchIn);

            if (invert)
                return !rege;

            return rege;
        }

        public static bool ContainsWildcard(string value) {

            return WildcardCharacters.Any(value.Contains);
        }
    }
}
