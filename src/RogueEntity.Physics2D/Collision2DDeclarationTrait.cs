using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Physics2D;

public class Collision2DDeclarationTrait<TSelfId, TOtherId>: IReferenceItemTrait<TSelfId>,
                                                             IBulkItemTrait<TSelfId>,
                                                             IItemComponentDesignTimeInformationTrait<Collision2DDeclaration<TSelfId, TOtherId>> 
    where TSelfId : struct, IEntityKey
{
    readonly List<MapLayer> targetLayers;
    public ItemTraitId Id { get; }
    public int Priority { get; }

    public Collision2DDeclarationTrait(MapLayer targetLayer)
    {
        Id = "Core.Items.Physics2D.CollidesWith";
        Priority = 100;
        this.targetLayers = new List<MapLayer>();
        this.targetLayers.Add(targetLayer);
    }

    public Collision2DDeclarationTrait(params MapLayer[] targetLayer)
    {
        Id = "Core.Items.Physics2D.CollidesWith";
        Priority = 100;
        this.targetLayers = new List<MapLayer>();
        foreach (var l in targetLayer)
        {
            this.targetLayers.Add(l);
        }
    }
    
    public IEnumerable<EntityRoleInstance> GetEntityRoles()
    {
        return Enumerable.Empty<EntityRoleInstance>();
    }

    public IEnumerable<EntityRelationInstance> GetEntityRelations()
    {
        yield return Physics2DModule.CollidesWithRelation.Instantiate<TSelfId, TOtherId>();
    }

    public void Initialize(IEntityViewControl<TSelfId> v, TSelfId k, IItemDeclaration item)
    {
    }

    public TSelfId Initialize(IItemDeclaration item, TSelfId reference)
    {
        return reference;
    }

    public void Apply(IEntityViewControl<TSelfId> v, TSelfId k, IItemDeclaration item)
    {
    }

    IReferenceItemTrait<TSelfId> IReferenceItemTrait<TSelfId>.CreateInstance()
    {
        return this;
    }

    IBulkItemTrait<TSelfId> IBulkItemTrait<TSelfId>.CreateInstance()
    {
        return this;
    }

    public bool TryQuery(out Collision2DDeclaration<TSelfId, TOtherId> t)
    {
        t = new Collision2DDeclaration<TSelfId, TOtherId>(this.targetLayers);
        return true;
    }
}