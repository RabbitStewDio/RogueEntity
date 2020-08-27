using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Equipment
{
    public class EquipmentSlotRequirementsTrait<TGameContext, TItemId> : StatelessItemComponentTraitBase<TGameContext, TItemId, EquipmentSlotRequirements> 
        where TItemId : IEntityKey
    {
        readonly EquipmentSlotRequirements r;

        public EquipmentSlotRequirementsTrait(EquipmentSlotRequirements r) : base("Core.Items.EquipmentRequirements", 500)
        {
            this.r = r;
        }

        protected override EquipmentSlotRequirements GetData(TGameContext context, TItemId k)
        {
            return r;
        }
    }
}