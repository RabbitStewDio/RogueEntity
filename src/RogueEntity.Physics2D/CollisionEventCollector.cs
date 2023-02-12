using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using System.Collections.Generic;

namespace RogueEntity.Physics2D;

public class CollisionEventCollector<TSelf, TOther>
    where TSelf : struct, IEntityKey
    where TOther : struct, IEntityKey
{
    readonly List<CollisionEventData<TOther>> events;
    TSelf self;

    public CollisionEventCollector()
    {
        events = new List<CollisionEventData<TOther>>();
    }

    public void Init(TSelf self)
    {
        this.self = self;
    }

    public void Clear()
    {
        events.Clear();
    }

    public void RecordCollision(TOther other, in Contact2D contact2D)
    {
        events.Add(new CollisionEventData<TOther>(other, contact2D));
    }

    public ReadOnlyListWrapper<CollisionEventData<TOther>> Events => events;
}