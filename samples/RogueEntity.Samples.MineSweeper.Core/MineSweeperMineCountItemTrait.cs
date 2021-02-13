using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Simple.MineSweeper
{
    public class MineSweeperMineCountItemTrait<TItemId> : SimpleBulkItemComponentTraitBase<TItemId, MineSweeperMineCount>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public MineSweeperMineCountItemTrait() : base("MineSweeper.MineCount", 100)
        {
        }

        protected override MineSweeperMineCount CreateInitialValue(TItemId reference)
        {
            return new MineSweeperMineCount();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return MineSweeperModule.MineFieldRole.Instantiate<TItemId>();
        }

        protected override bool TryUpdateBulkData(TItemId k, in MineSweeperMineCount data, out TItemId changedK)
        {
            changedK = k.WithData(data.Count);
            return true;
        }
    }
}
