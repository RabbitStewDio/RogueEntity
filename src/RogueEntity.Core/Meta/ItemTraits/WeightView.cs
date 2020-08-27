namespace RogueEntity.Core.Meta.ItemTraits
{
    public readonly struct WeightView
    {
        public readonly Weight BaseWeight;
        public readonly Weight InventoryWeight;
        public readonly Weight EquipmentWeight;
        public readonly Weight MaximumCarryWeight;
        public Weight TotalWeight => BaseWeight + InventoryWeight + EquipmentWeight;
        public Weight TotalCarriedWeight => InventoryWeight + EquipmentWeight;

        public WeightView(Weight baseWeight, Weight inventoryWeight, Weight equipmentWeight, Weight totalCarryWeight)
        {
            BaseWeight = baseWeight;
            InventoryWeight = inventoryWeight;
            EquipmentWeight = equipmentWeight;
            MaximumCarryWeight = totalCarryWeight;
        }
    }
}