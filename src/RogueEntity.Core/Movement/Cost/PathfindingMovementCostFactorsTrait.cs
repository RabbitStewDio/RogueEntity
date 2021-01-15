using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.MovementModes;

namespace RogueEntity.Core.Movement.Cost
{
    public class PathfindingMovementCostFactorsTrait< TActorId> : IItemComponentInformationTrait< TActorId, PathfindingMovementCostFactors>,
                                                                               IBulkItemTrait< TActorId>,
                                                                               IReferenceItemTrait< TActorId>
        where TActorId : IEntityKey
    {
        readonly BufferList<IItemComponentInformationTrait< TActorId, MovementCost>> sourceTraits;
        readonly List<MovementCost> movementCosts;
        bool movementCostsValid;
        bool traitsValid;

        public PathfindingMovementCostFactorsTrait()
        {
            sourceTraits = new BufferList<IItemComponentInformationTrait< TActorId, MovementCost>>();
            movementCosts = new List<MovementCost>();
        }

        public ItemTraitId Id => "Core.Trait.Movement.PathfindingMovementCostFactor";
        public int Priority => 100;
        
        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return MovementModules.GeneralMovableActorRole.Instantiate<TActorId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }

        public bool TryQuery(IEntityViewControl<TActorId> v,  TActorId k, out PathfindingMovementCostFactors t)
        {

            if (movementCostsValid)
            {
                t = new PathfindingMovementCostFactors(movementCosts);
                return true;
            }
            
            movementCosts.Clear();
            foreach (var trait in sourceTraits)
            {
                if (trait.TryQuery(v,  k, out var movementCost))
                {
                    movementCosts.Add(movementCost);
                }
            }
            movementCosts.Sort();
            movementCostsValid = true;
            t = new PathfindingMovementCostFactors(movementCosts);
            return movementCosts.Count > 0;
        }

        PathfindingMovementCostFactorsTrait< TActorId> CreateInstance() => new PathfindingMovementCostFactorsTrait< TActorId>();

        public TActorId Initialize( IItemDeclaration item, TActorId reference)
        {
            if (!traitsValid)
            {
                item.QueryAll(sourceTraits);
                traitsValid = true;
            }

            return reference;
        }

        public void Initialize(IEntityViewControl<TActorId> v,  TActorId k, IItemDeclaration item)
        {
            Initialize( item, k);
        }

        public void Apply(IEntityViewControl<TActorId> v,  TActorId k, IItemDeclaration item)
        {
            movementCosts.Clear();
            movementCostsValid = false;
        }

        IBulkItemTrait< TActorId> IBulkItemTrait< TActorId>.CreateInstance()
        {
            return CreateInstance();
        }

        IReferenceItemTrait< TActorId> IReferenceItemTrait< TActorId>.CreateInstance()
        {
            return CreateInstance();
        }
    }
}