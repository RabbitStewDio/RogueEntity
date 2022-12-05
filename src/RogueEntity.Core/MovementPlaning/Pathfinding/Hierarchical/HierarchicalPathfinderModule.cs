using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.MovementModes;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

public class HierarchicalPathfinderModule : ModuleBase
{
    public static readonly string ModuleId = "Core.MovementPlaning.Pathfinding.HierarchicalPathfinder";
    public static readonly EntitySystemId RegisterPathfinderServiceId = "System.Core.MovementPlaning.Pathfinding.HierarchicalPathfinder.RegisterService";
    public static readonly EntitySystemId RegisterPathfinderServiceFinalizerId = "System.Core.MovementPlaning.Pathfinding.HierarchicalPathfinder.RegisterServiceFinalizer";
    public static readonly EntityRole HierarchicalPathfindingActorRole = new EntityRole("Role.Core.MovementPlaning.HierarchicalPathfindingActor");

    public HierarchicalPathfinderModule()
    {
        Id = ModuleId;
        Author = "RogueEntity.Core";
        Name = "RogueEntity Core Module - Movement Planing - Hierarchical Pathfinding Module";
        Description = "Provides an optimized pathfinder for large maps";
        IsFrameworkModule = true;

        DeclareDependencies(ModuleDependency.Of(SingleLevelPathfinderModule.ModuleId));

        RequireRole(HierarchicalPathfindingActorRole)
            .WithImpliedRole(SingleLevelPathfinderModule.PathfindingActorRole)
            .WithImpliedRole(MovementModules.GeneralMovableActorRole);
    }

    [EntityRoleInitializer("Role.Core.MovementPlaning.PathfindingActor")]
    protected void InitializePlayerObserverRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                         IModuleInitializer initializer,
                                                         EntityRole r)
        where TItemId : struct, IEntityKey
    {
        var entityContext = initializer.DeclareEntityContext<TItemId>();
        entityContext.Register(RegisterPathfinderServiceId, -19_500, RegisterService);
        entityContext.Register(RegisterPathfinderServiceFinalizerId, -18_050, RegisterServiceFinalizer);
    }

    void RegisterService<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                    IGameLoopSystemRegistration context,
                                    EntityRegistry<TEntityId> registry) where TEntityId : struct, IEntityKey
    {
        var serviceResolver = initParameter.ServiceResolver;
        if (serviceResolver.TryResolve<HierarchicalPathfinderSource>(out var source))
        {
            return;
        }

        if (!serviceResolver.TryResolve<SingleLevelPathFinderSource>(out var fragmentSource))
        {
            return;
        }

        if (!serviceResolver.TryResolve<IMovementDataProvider>(out var movementDataProvider))
        {
            throw new InvalidOperationException("IMovementDataProvider has not been registered");
        }

        if (!serviceResolver.TryResolve<MovementModeRegistry>(out var reg))
        {
            throw new InvalidOperationException("MovementSystemRegistry has not been registered");
        }

        var gridConfig = PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver);
        if (!serviceResolver.TryResolve<HierarchicalPathfindingSystemCollection>(out var highLevelData))
        {
            highLevelData = new HierarchicalPathfindingSystemCollection(gridConfig, movementDataProvider, fragmentSource);
            serviceResolver.Store(highLevelData);
        }

        foreach (var movementMode in reg.Modes)
        {
            var movementStyles = CollectMovementTypes(initParameter.ContentDeclarations, movementMode);
            foreach (var style in movementStyles)
            {
                highLevelData.RegisterMovementCombination(movementMode, style);
            }
        }

        highLevelData.Initialize();

        if (!serviceResolver.TryResolve<SingleLevelPathPool>(out var pathPool))
        {
            pathPool = new SingleLevelPathPool();
            serviceResolver.Store(pathPool);
        }
        
        if (!serviceResolver.TryResolve<HierarchicalPathFinderPolicy>(out var policy))
        {
            policy = new HierarchicalPathFinderPolicy(highLevelData, pathPool);
        }

        source = new HierarchicalPathfinderSource(fragmentSource, policy, movementDataProvider);
        serviceResolver.Store(source);
    }

    HashSet<DistanceCalculation> CollectMovementTypes<TEntity>(IModuleContentDeclarations<TEntity> contentDeclarations,
                                                                IMovementMode movementMode) 
        where TEntity : struct, IEntityKey
    {
        var calcs = new HashSet<DistanceCalculation>();
        foreach (var c in contentDeclarations.DeclaredReferenceItems)
        {
            if (c.itemDeclaration.TryQuery<IMovementStyleInformationTrait>(out var trait) &&
                trait.TryQuery(out var movementStyleData))
            {
                var (mode, style) = movementStyleData;
                if (movementMode.Equals(mode))
                {
                    calcs.Add(style);
                }
            }
        }

        foreach (var c in contentDeclarations.DeclaredBulkItems)
        {
            if (c.itemDeclaration.TryQuery<IMovementStyleInformationTrait>(out var trait) &&
                trait.TryQuery(out var movementStyleData))
            {
                var (mode, style) = movementStyleData;
                if (movementMode.Equals(mode))
                {
                    calcs.Add(style);
                }
            }
        }

        return calcs;
    }

    void RegisterServiceFinalizer<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                             IGameLoopSystemRegistration context,
                                             EntityRegistry<TEntityId> registry)
        where TEntityId : struct, IEntityKey
    {
        var serviceResolver = initParameter.ServiceResolver;
        if (!serviceResolver.TryResolve<SingleLevelPathFinderSource>(out var source))
        {
            return;
        }

        if (serviceResolver.TryResolve<IPathFinderSource>(out _))
        {
            return;
        }

        serviceResolver.Store<IPathFinderSource>(source);
    }
}