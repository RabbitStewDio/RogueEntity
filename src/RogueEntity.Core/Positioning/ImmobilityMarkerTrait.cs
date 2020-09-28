using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Positioning
{
    public class ImmobilityMarkerTrait<TGameContext, TItemId> : StatelessItemComponentTraitBase<TGameContext, TItemId, ImmobilityMarker>
        where TItemId : IEntityKey
    {
        public ImmobilityMarkerTrait(): base("Item.Generic.Positional", 100)
        {
        }

        protected override ImmobilityMarker GetData(TGameContext context, TItemId k)
        {
            return new ImmobilityMarker();
        }
    }
}