using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.MovementModes;

namespace RogueEntity.Core.Movement.Cost
{
    public class AggregateMovementCostFactorsTrait<TActorId> : IItemComponentInformationTrait<TActorId, AggregateMovementCostFactors>,
                                                               IBulkItemTrait<TActorId>,
                                                               IReferenceItemTrait<TActorId>
        where TActorId : struct, IEntityKey
    {
        readonly BufferList<IMovementCostTrait<TActorId>> sourceTraits;
        readonly List<MovementCost> movementCosts;
        bool movementCostsValid;
        bool traitsValid;

        public AggregateMovementCostFactorsTrait()
        {
            sourceTraits = new BufferList<IMovementCostTrait<TActorId>>();
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

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, out AggregateMovementCostFactors t)
        {
            if (movementCostsValid)
            {
                t = new AggregateMovementCostFactors(movementCosts);
                return true;
            }

            movementCosts.Clear();
            foreach (var trait in sourceTraits)
            {
                if (trait.TryQuery(v, k, out var movementCost))
                {
                    movementCosts.Add(movementCost);
                }
            }

            movementCosts.Sort();
            movementCostsValid = true;
            t = new AggregateMovementCostFactors(movementCosts);
            return movementCosts.Count > 0;
        }

        AggregateMovementCostFactorsTrait<TActorId> CreateInstance() => new AggregateMovementCostFactorsTrait<TActorId>();

        public TActorId Initialize(IItemDeclaration item, TActorId reference)
        {
            if (!traitsValid)
            {
                item.QueryAll(sourceTraits);
                traitsValid = true;
            }

            return reference;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            Initialize(item, k);
        }

        public void Apply(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            movementCosts.Clear();
            movementCostsValid = false;
        }

        IBulkItemTrait<TActorId> IBulkItemTrait<TActorId>.CreateInstance()
        {
            return CreateInstance();
        }

        IReferenceItemTrait<TActorId> IReferenceItemTrait<TActorId>.CreateInstance()
        {
            return CreateInstance();
        }
    }
}
