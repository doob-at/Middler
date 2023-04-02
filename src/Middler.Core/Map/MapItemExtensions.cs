using System;
using System.Collections.Generic;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Models;
using Microsoft.Extensions.DependencyInjection;
using NamedServices.Microsoft.Extensions.DependencyInjection;

namespace doob.Middler.Core.Map
{
    public static class MapItemExtensions {

        public static IEnumerable<MiddlerRule> GetRules(this MapItem item, IServiceProvider serviceProvider)
        {

            //if (item.Rule == null && item.RepoType == null)
            //    throw new ArgumentNullException(nameof(item.RepoType));

            //if (item.Rule == null && item.RepoName == null)
            //    throw new ArgumentNullException(nameof(item.RepoName));

            //if (item.Rule == null && item.RepoName != null)
            //    throw new ArgumentNullException(nameof(item.Rule));

            switch (item.ItemType) {
                case MapItemType.NamedRepo: {
                    var repo = serviceProvider.GetRequiredNamedService<IMiddlerRepository>(item.RepoName!);
                    return repo.ProvideRules();
                }
                case MapItemType.Repo: {
                    var repo = (IMiddlerRepository)serviceProvider.GetRequiredService(item.RepoType!);
                    return repo.ProvideRules();
                }
                case MapItemType.Rule: {
                    return new List<MiddlerRule>() { item.Rule! };
                }
            }

            return ArraySegment<MiddlerRule>.Empty;
        }
    }
}