using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Samples.MineSweeper.Core.Traits
{
    public class MineSweeperMineCountItemTrait<TItemId> : SimpleBulkItemComponentTraitBase<TItemId, MineSweeperMineCount>
        where TItemId : struct, IBulkDataStorageKey<TItemId>
    {
        public MineSweeperMineCountItemTrait() : base("MineSweeper.MineCount", 100)
        {
        }

        protected override MineSweeperMineCount CreateInitialValue(TItemId reference)
        {
            return new MineSweeperMineCount(0);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return MineSweeperModule.MineFieldRole.Instantiate<TItemId>();
        }

        protected override bool TryQueryBulkData(IEntityViewControl<TItemId> v, TItemId k, out MineSweeperMineCount t)
        {
            t = new MineSweeperMineCount(k.Data);
            return true;
        }

        protected override bool TryUpdateBulkData(TItemId k, in MineSweeperMineCount data, out TItemId changedK)
        {
            changedK = k.WithData(data.Count);
            return true;
        }
    }
}
