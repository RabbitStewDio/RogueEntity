using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Players;
using System.Collections.Generic;

namespace RogueEntity.Samples.MineSweeper.Core
{
    public class MineSweeperPlayerDataTrait<TActorId>: SimpleReferenceItemComponentTraitBase<TActorId, MineSweeperPlayerData>
        where TActorId : IEntityKey
    {
        public MineSweeperPlayerDataTrait() : base("ItemTrait.MineSweeper.PlayerData", 100)
        {
        }

        protected override Optional<MineSweeperPlayerData> CreateInitialValue(TActorId reference)
        {
            return new MineSweeperPlayerData();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return PlayerModule.PlayerRole.Instantiate<TActorId>();
        }
    }
}
