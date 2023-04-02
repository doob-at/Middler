using System.Collections.Generic;
using System.Linq;
using doob.Reflectensions;

namespace doob.Middler.Common.SharedModels.Models
{
    public class MiddlerRouteQueryParameters : ExpandableObject {

        private List<RuleQueryParameter> QueryParameters { get; } = new();

        
        public MiddlerRouteQueryParameters(IDictionary<string, string?> dict) : base(dict) {


        }

        public MiddlerRouteQueryParameters(IDictionary<string, string?> dict, List<RuleQueryParameter> queryParameters) : this(dict) {
            QueryParameters = queryParameters;
        }
        

        public ExpandableObject Others() {
            var definedParams = QueryParameters.Select(q => q.Name);
            var others = GetProperties()
                .Where(kvp => !definedParams.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return new ExpandableObject(others);
        }


    }
}