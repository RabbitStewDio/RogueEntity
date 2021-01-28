using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Meta.Items
{
    public class FlagItemTrait<TEntityId, TFlagItem> : SimpleReferenceItemComponentTraitBase<TEntityId, TFlagItem>
        where TEntityId : IEntityKey
        where TFlagItem : new()
    {
        readonly bool flagExistsOnNewItems;

        public FlagItemTrait(bool flagExistsOnNewItems, ItemTraitId id, int priority = 100) : base(id, priority)
        {
            this.flagExistsOnNewItems = flagExistsOnNewItems;
        }

        protected override TFlagItem CreateInitialValue(TEntityId reference)
        {
            return new TFlagItem();
        }

        public override void Initialize(IEntityViewControl<TEntityId> v, TEntityId k, IItemDeclaration item)
        {
            if (flagExistsOnNewItems)
            {
                base.Initialize(v, k, item);
            }
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }
    }
}
