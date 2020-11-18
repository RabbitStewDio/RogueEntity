using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Movement.CostModifier
{
    public struct PathfindingMovementCostFactors
    {
        public readonly ReadOnlyListWrapper<MovementCost> MovementCosts;

        public PathfindingMovementCostFactors(ReadOnlyListWrapper<MovementCost> movementCosts)
        {
            MovementCosts = movementCosts;
        }
    }

    public class PathfindingMovementCostFactorsTrait<TGameContext, TActorId> : IItemComponentInformationTrait<TGameContext, TActorId, PathfindingMovementCostFactors>,
                                                                               IBulkItemTrait<TGameContext, TActorId>,
                                                                               IReferenceItemTrait<TGameContext, TActorId>
        where TActorId : IEntityKey
    {
        readonly List<IItemComponentInformationTrait<TGameContext, TActorId, MovementCost>> sourceTraits;
        readonly List<MovementCost> movementCosts;
        bool movementCostsValid;
        bool traitsValid;

        public PathfindingMovementCostFactorsTrait()
        {
            sourceTraits = new List<IItemComponentInformationTrait<TGameContext, TActorId, MovementCost>>();
            movementCosts = new List<MovementCost>();
        }

        public ItemTraitId Id => "Core.Trait.Movement.PathfindingMovementCostFactor";
        public int Priority => 100;
        
        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out PathfindingMovementCostFactors t)
        {

            if (movementCostsValid)
            {
                t = new PathfindingMovementCostFactors(movementCosts);
                return true;
            }
            
            movementCosts.Clear();
            foreach (var trait in sourceTraits)
            {
                if (trait.TryQuery(v, context, k, out var movementCost))
                {
                    movementCosts.Add(movementCost);
                }
            }
            movementCosts.Sort();
            movementCostsValid = true;
            t = new PathfindingMovementCostFactors(movementCosts);
            return movementCosts.Count > 0;
        }

        PathfindingMovementCostFactorsTrait<TGameContext, TActorId> CreateInstance() => new PathfindingMovementCostFactorsTrait<TGameContext, TActorId>();

        public TActorId Initialize(TGameContext context, IItemDeclaration item, TActorId reference)
        {
            if (!traitsValid)
            {
                item.QueryAll(sourceTraits);
                traitsValid = true;
            }

            return reference;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            Initialize(context, item, k);
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            movementCosts.Clear();
            movementCostsValid = false;
        }

        IBulkItemTrait<TGameContext, TActorId> IBulkItemTrait<TGameContext, TActorId>.CreateInstance()
        {
            return CreateInstance();
        }

        IReferenceItemTrait<TGameContext, TActorId> IReferenceItemTrait<TGameContext, TActorId>.CreateInstance()
        {
            return CreateInstance();
        }
    }
}