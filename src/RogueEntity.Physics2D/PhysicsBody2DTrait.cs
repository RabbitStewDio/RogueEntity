using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Physics2D.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Physics2D;

public enum PhysicsBodyType
{
    Static, 
    Sensor,
    Dynamic
}

public readonly struct PhysicsBody2D
{
    public readonly IPhysicsShape2D Shape;
    public readonly PhysicsBodyType BodyType;

    public PhysicsBody2D(IPhysicsShape2D shape, PhysicsBodyType bodyType)
    {
        Shape = shape;
        BodyType = bodyType;
    }
}

public class PhysicsBody2DTrait<TItemId> : IReferenceItemTrait<TItemId>,
                                           IBulkItemTrait<TItemId>,
                                           IItemComponentInformationTrait<TItemId, PhysicsBody2D>
    where TItemId : struct, IEntityKey
{
    public ItemTraitId Id { get; }
    public int Priority { get; }

    public PhysicsBody2DTrait(PhysicsBody2D body)
    {
        if (body.Shape == null)
        {
            throw new ArgumentNullException();
        }
        
        this.Body = body;
        Id = "Core.Items.Physics2D.Body";
        Priority = 100;
    }

    public PhysicsBody2D Body { get; }

    public void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
    {
    }

    public void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
    {
    }

    public TItemId Initialize(IItemDeclaration item, TItemId reference)
    {
        return reference;
    }

    IBulkItemTrait<TItemId> IBulkItemTrait<TItemId>.CreateInstance()
    {
        return this;
    }

    IReferenceItemTrait<TItemId> IReferenceItemTrait<TItemId>.CreateInstance()
    {
        return this;
    }

    public IEnumerable<EntityRoleInstance> GetEntityRoles()
    {
        yield return Physics2DModule.Physics2DEntityRole.Instantiate<TItemId>();
    }

    public IEnumerable<EntityRelationInstance> GetEntityRelations()
    {
        return Enumerable.Empty<EntityRelationInstance>();
    }

    public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out PhysicsBody2D t)
    {
        t = Body;
        return true;
    }
}