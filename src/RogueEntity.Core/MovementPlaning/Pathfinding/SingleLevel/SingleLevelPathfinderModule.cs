using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.MovementModes;
using RogueEntity.Core.Positioning;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;

public class SingleLevelPathfinderModule: ModuleBase
{
    public static readonly string ModuleId = "Core.MovementPlaning.Pathfinding.SingleLevel";
    public static readonly EntitySystemId RegisterSingleLevelPathfinderServiceId = "System.Core.MovementPlaning.Pathfinding.SingleLevel.RegisterService";
    public static readonly EntitySystemId RegisterPathfinderServiceId = "System.Core.MovementPlaning.Pathfinding.SingleLevel.RegisterServiceFinalizer";
    public static readonly EntityRole PathfindingActorRole = new EntityRole("Role.Core.MovementPlaning.PathfindingActor");

    public SingleLevelPathfinderModule()
    {
        Id = ModuleId;
        Author = "RogueEntity.Core";
        Name = "RogueEntity Core Module - Movement Planning - Single Level Pathfinding";
        Description = "Provides base classes and behaviours for navigating flat levels ";
        IsFrameworkModule = true;
        
        DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));

        RequireRole(PathfindingActorRole).WithImpliedRole(MovementModules.GeneralMovableActorRole);
    }
    
    [EntityRoleInitializer("Role.Core.MovementPlaning.PathfindingActor")]
    protected void InitializePlayerObserverRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                         IModuleInitializer initializer,
                                                         EntityRole r)
        where TItemId : struct, IEntityKey
    {
        var entityContext = initializer.DeclareEntityContext<TItemId>();
        entityContext.Register(RegisterSingleLevelPathfinderServiceId, -20_000, RegisterService);
        entityContext.Register(RegisterPathfinderServiceId, -18_100, RegisterServiceFinalizer);
    }

    void RegisterService<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter, 
                                    IGameLoopSystemRegistration context, 
                                    EntityRegistry<TEntityId> registry) where TEntityId : struct, IEntityKey
    {
        var serviceResolver = initParameter.ServiceResolver;
        if (!serviceResolver.TryResolve<SingleLevelPathFinderSource>(out var source))
        {
            if (!serviceResolver.TryResolve<SingleLevelPathFinderPolicy>(out var policy))
            {
                var gridConfig = PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver);
                policy = new SingleLevelPathFinderPolicy(gridConfig);
            }


            if (!serviceResolver.TryResolve<IMovementDataProvider>(out var movementDataProvider))
            {
                throw new InvalidOperationException();
            }

            source = new SingleLevelPathFinderSource(policy, movementDataProvider);
            serviceResolver.Store(source);
        }
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