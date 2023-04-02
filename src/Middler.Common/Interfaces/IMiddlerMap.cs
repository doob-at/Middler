using System;
using System.Collections.Generic;
using doob.Middler.Common.SharedModels.Models;

namespace doob.Middler.Common.Interfaces
{
    public interface IMiddlerMap
    {
        List<MiddlerRule> GetFlatList(IServiceProvider serviceProvider);
        void AddRule(params MiddlerRule[] middlerRules);
        void AddRepo<T>();
        void AddNamedRepo(string name);
    }
}
