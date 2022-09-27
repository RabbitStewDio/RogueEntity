using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.Naming
{
    public class DefaultItemNameTrait<TItemId> : StatelessItemComponentTraitBase<TItemId, IDisplayName>
        where TItemId : struct, IEntityKey
    {
        public DefaultItemNameTrait(IDisplayName displayName) : base("Core.Item.DisplayName", 100)
        {
            this.BaseValue = displayName;
        }

        protected override IDisplayName GetData(TItemId k)
        {
            return BaseValue;
        }

        public IDisplayName BaseValue { get; }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.EntityRole.Instantiate<TItemId>();
        }
    }
}
