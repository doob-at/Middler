using System.Collections.Generic;
using doob.Middler.Common.SharedModels.Enums;
using doob.Middler.Common.SharedModels.Models;

namespace doob.Middler.Core.Models
{
    public record MiddlerRuleMatch
    {
        internal AccessMode AccessMode { get; }
        internal MiddlerRule MiddlerRule { get; }
        //internal Dictionary<string, object> RouteData { get; set; }
        internal List<MiddlerRule> RemainingEndpointInfos { get; set; } = new List<MiddlerRule>();

        public MiddlerRuleMatch(MiddlerRule middlerRule, AccessMode accessMode)
        {
            MiddlerRule = middlerRule;
            AccessMode = accessMode;
        }
    }
}
