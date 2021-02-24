using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;

namespace RogueEntity.Core.Movement.MovementModes
{
    public static class MovementModules
    {
        public static readonly EntityRole GeneralMovableActorRole = new EntityRole("Role.Core.Movement.MovableActor");
        public static readonly EntityRole GeneralCostModifierSourceRole = new EntityRole("Role.Core.Movement.CostModifierSource");

        public static EntityRole GetMovableActorWithPointsRole<TMovementMode>() => new EntityRole($"Role.Core.Movement.{typeof(TMovementMode).Name}.MovableActor.Points");
        public static EntityRole GetMovableActorWithVelocityRole<TMovementMode>() => new EntityRole($"Role.Core.Movement.{typeof(TMovementMode).Name}.MovableActor.Velocity");
        
        public static EntityRole GetMovableActorRole<TMovementMode>() => new EntityRole($"Role.Core.Movement.{typeof(TMovementMode).Name}.MovableActor");
        public static EntityRole GetCostModifierSourceRole<TMovementMode>() => new EntityRole($"Role.Core.Movement.{typeof(TMovementMode).Name}.CostModifierSource");

        public static EntityRelation GetCostModifierRelation<TMovementMode>() => new EntityRelation($"Relation.Core.Movement.Resistance.{typeof(TMovementMode).Name}.ProvidesCostData",
                                                                                                    GetCostModifierSourceRole<TMovementMode>(), GetMovableActorRole<TMovementMode>());

        public static EntitySystemId CreateSystemId<TMovementMode>(string job) => new EntitySystemId($"Core.Systems.Movement.CostModifier.{typeof(TMovementMode).Name}.{job}");
        public static EntitySystemId CreateEntityId<TMovementMode>() => new EntitySystemId($"Entities.Systems.Movement.CostModifier.{typeof(TMovementMode).Name}");
        
        public static EntitySystemId CreateActorEntityId<TMovementMode>() => new EntitySystemId($"Entities.Systems.Movement.MovementCost.{typeof(TMovementMode).Name}");
    }
}