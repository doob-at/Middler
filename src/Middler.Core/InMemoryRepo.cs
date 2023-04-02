using System.Collections.Generic;
using System.Linq;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Models;

namespace doob.Middler.Core
{
    public class InMemoryRepo: IMiddlerRepository
    {

        private List<MiddlerRule> Endpoints { get; }


        public InMemoryRepo(): this(null)
        {

        }

        public InMemoryRepo(IEnumerable<MiddlerRule>? middlerRules)
        {

            Endpoints = middlerRules?.ToList() ?? new List<MiddlerRule>();
        }

        public List<MiddlerRule> ProvideRules()
        {
            return Endpoints;
        }


        internal void AddRule(params MiddlerRule[] middlerRules)
        {
            Endpoints.AddRange(middlerRules);
        }
    }
}
