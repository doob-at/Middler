using System.Collections.Generic;
using doob.Middler.Common.SharedModels.Models;

namespace doob.Middler.Common.Interfaces
{
    public interface IMiddlerRepository {
        
        List<MiddlerRule> ProvideRules();
    }
   
}
