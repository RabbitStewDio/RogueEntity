using RogueEntity.Api.ItemTraits;
using System.Collections.Generic;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentDesignTimeInformationTrait<TComponent> : IItemTrait
    {
        bool TryQuery(out TComponent t);
    }

    public static class DesignTimeDataExtensions
    {
        public static IEnumerable<(IItemDeclaration, TDesignTimeData)> QueryDesignTimeTrait<TDesignTimeData>(this IItemRegistry r)
        {
            foreach (var i in r.Items)
            {
                if (i.TryQuery(out IItemComponentDesignTimeInformationTrait<TDesignTimeData> t))
                {
                    if (t.TryQuery(out var data))
                    {
                        yield return (i, data);
                    }
                }
            }
        }

    }
}