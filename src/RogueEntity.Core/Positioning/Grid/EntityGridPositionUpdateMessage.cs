namespace RogueEntity.Core.Positioning.Grid
{
    public readonly struct EntityGridPositionUpdateMessage
    {
        public readonly EntityGridPosition Data;

        public EntityGridPositionUpdateMessage(EntityGridPosition data)
        {
            Data = data;
        }

        public static EntityGridPositionUpdateMessage From<TPosition>(in TPosition p)
            where TPosition : IPosition<TPosition>
        {
            return new EntityGridPositionUpdateMessage(EntityGridPosition.From(p));
        }
    }
}
