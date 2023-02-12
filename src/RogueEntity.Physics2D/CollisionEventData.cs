namespace RogueEntity.Physics2D;

public readonly struct CollisionEventData<TOther>
{
    public readonly TOther Other;
    public readonly Contact2D Contact2D;

    public CollisionEventData(TOther other, Contact2D contact2D)
    {
        this.Other = other;
        this.Contact2D = contact2D;
    }
}