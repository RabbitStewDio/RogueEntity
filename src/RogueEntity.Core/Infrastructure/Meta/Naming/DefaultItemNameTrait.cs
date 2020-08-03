using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Meta.Naming
{
    public class DefaultItemNameTrait<TGameContext, TItemId> : StatelessItemComponentTraitBase<TGameContext, TItemId, IDisplayName>, 
                                                      IItemComponentInformationTrait<IDisplayName> 
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public DefaultItemNameTrait(IDisplayName displayName): base("Core.Item.DisplayName", 100)
        {
            this.BaseValue = displayName;
        }

        protected override IDisplayName GetData(TGameContext context, TItemId k)
        {
            return BaseValue;
        }

        public IDisplayName BaseValue { get; }
    }
}