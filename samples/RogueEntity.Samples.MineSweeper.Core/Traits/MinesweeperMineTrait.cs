using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Samples.MineSweeper.Core.Traits;

public readonly struct Mine
{
}

public class MinesweeperMineTrait<TItemId>: IReferenceItemTrait<TItemId>,
                                            IItemComponentInformationTrait<TItemId, Mine> 
    where TItemId : struct, IEntityKey
{
    public ItemTraitId Id { get; }
    public int Priority { get; }

    public MinesweeperMineTrait()
    {
        Id = "ItemTrait.MineSweeper.Mine";
        Priority = 100;
    }

    public IEnumerable<EntityRoleInstance> GetEntityRoles()
    {
        yield return MineSweeperModule.MineRole.Instantiate<TItemId>();
    }

    public IEnumerable<EntityRelationInstance> GetEntityRelations()
    {
        return Enumerable.Empty<EntityRelationInstance>();
    }

    public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out Mine t)
    {
        t = default;
        return true;
    }

    public void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
    {
    }

    public void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
    {
    }

    public IReferenceItemTrait<TItemId> CreateInstance()
    {
        return new MinesweeperMineTrait<TItemId>();
    }
}