using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Receptors.Noise;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Sensing.Sources.Noise;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Tests.Fixtures;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Sensing.Discovery
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public class DiscoveryMapSystemTest
    {
        const string EmptyRoom = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string EmptyRoomSenseMapA = @"
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
";

        const string EmptyRoomSenseMapB = @"
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., 1, 1, 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., 1, 1, 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., 1, 1, 1, 1, 1, ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
";

        const string EmptyRoomSenseMapAfterMove = @"
., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, ., 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., 1, 1, 1, 1, 1, 1, 1, 1
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .
 ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., ., .";

        ItemDeclarationId senseReceptorActive10;
        ItemDeclarationId senseReceptorActive5;

        protected SenseMappingTestContext context;
        List<Action> senseSystemActions;
        DiscoveryMapSystem senseSystem;


        [SetUp]
        public void SetUp()
        {
            context = new SenseMappingTestContext();
            context.ItemEntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            context.ItemEntityRegistry.RegisterFlag<ImmobilityMarker>();
            context.ItemEntityRegistry.RegisterNonConstructable<DiscoveryMapData>();

            context.ItemEntityRegistry.RegisterNonConstructable<TemperatureSense>();
            context.ItemEntityRegistry.RegisterNonConstructable<SenseSourceState<TemperatureSense>>();
            context.ItemEntityRegistry.RegisterFlag<ObservedSenseSource<TemperatureSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseDirtyFlag<TemperatureSense>>();

            context.ItemEntityRegistry.RegisterNonConstructable<NoiseSense>();
            context.ItemEntityRegistry.RegisterNonConstructable<SenseSourceState<NoiseSense>>();
            context.ItemEntityRegistry.RegisterFlag<ObservedSenseSource<NoiseSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseDirtyFlag<NoiseSense>>();

            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorData<VisionSense, TemperatureSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorState<VisionSense, TemperatureSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<VisionSense, TemperatureSense>>();

            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorData<NoiseSense, NoiseSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorState<NoiseSense, NoiseSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<NoiseSense, NoiseSense>>();

            var visionSensePhysics =
                new InfraVisionSenseReceptorPhysicsConfiguration(new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0)));
            var noiseSensePhysics =
                new NoiseSenseReceptorPhysicsConfiguration(new NoisePhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)), new FloodFillWorkingDataSource());

            senseReceptorActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseReceptor-Active-10")
                                                                  .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(TestMapLayers.One))
                                                                  .WithTrait(new DiscoveryMapTrait<ItemReference>())
                                                                  .WithTrait(new InfraVisionSenseTrait<ItemReference>(visionSensePhysics, 10))
            );
            senseReceptorActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseReceptor-Active-5")
                                                                 .WithTrait(
                                                                     new ReferenceItemGridPositionTrait<ItemReference>(TestMapLayers.One))
                                                                 .WithTrait(new DiscoveryMapTrait<ItemReference>())
                                                                 .WithTrait(new NoiseDirectionSenseTrait<ItemReference>(noiseSensePhysics, 10))
            );

            senseSystem = new DiscoveryMapSystem();

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
            mapData.TryGetWritableView(0, out _, DataViewCreateMode.CreateMissing).Should().BeTrue();
        }

        protected virtual List<Action> CreateSystemActions()
        {
            var builder = context.ItemEntityRegistry.BuildSystem().WithoutContext();

            return new List<Action>
            {
                builder.WithInputParameter<DiscoveryMapData,
                           SensoryReceptorState<VisionSense, TemperatureSense>,
                           SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>,
                           SenseReceptorDirtyFlag<VisionSense, TemperatureSense>>()
                       .CreateSystem(senseSystem.ExpandDiscoveredArea),

                builder.WithInputParameter<DiscoveryMapData,
                           SensoryReceptorState<NoiseSense, NoiseSense>,
                           SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense>,
                           SenseReceptorDirtyFlag<NoiseSense, NoiseSense>>()
                       .CreateSystem(senseSystem.ExpandDiscoveredArea)
            };
        }

        protected SenseSourceData ComputeDummySourceData(int radius)
        {
            var sd = new SenseSourceData(10);
            foreach (var p in sd.Bounds.Contents)
            {
                var str = radius - (float)DistanceCalculation.Euclid.Calculate2D(p);
                if (str > 0)
                {
                    sd.Write(p, new Position2D(0, 0), str);
                }
            }

            sd.Write(new Position2D(0, 0), new Position2D(0, 0), radius);
            sd.MarkWritten();
            return sd;
        }

        protected SenseDataMap ComputeDummySenseMap(int radius, Position2D origin)
        {
            var sd = new SenseDataMap();
            var bounds = new Rectangle(origin, radius, radius);
            foreach (var p in bounds.Contents)
            {
                var str = radius - (float)DistanceCalculation.Euclid.Calculate2D(p - origin);
                if (str > 0)
                {
                    sd.Write(p, origin, str);
                }
            }

            sd.Write(origin, origin, radius);
            return sd;
        }


        /// <summary>
        ///   The sensory receptor's field of view is calculated in another system. Let's simply provide precalculated values here.
        /// </summary>
        /// <param name="active10"></param>
        /// <param name="active5"></param>
        protected virtual void PrepareReceptorItems(ItemReference active10, ItemReference active5)
        {
            context.ItemPlacementService.TryPlaceItem(active10, Position.Of(TestMapLayers.One, 26, 4)).Should().BeTrue();
            context.ItemPlacementService.TryPlaceItem(active5, Position.Of(TestMapLayers.One, 7, 9)).Should().BeTrue();

            context.ItemEntityRegistry.AssignComponent<SenseReceptorDirtyFlag<VisionSense, TemperatureSense>>(active10);
            context.ItemEntityRegistry.AssignComponent<SenseReceptorDirtyFlag<NoiseSense, NoiseSense>>(active5);

            context.ItemEntityRegistry.AssignOrReplace(active10,
                                                       new SensoryReceptorState<VisionSense, TemperatureSense>(ComputeDummySourceData(10),
                                                                                                               SenseSourceDirtyState.Active, Position.Of(TestMapLayers.One, 26, 4), 10));
            context.ItemEntityRegistry.AssignOrReplace(active5,
                                                       new SensoryReceptorState<NoiseSense, NoiseSense>(ComputeDummySourceData(5),
                                                                                                        SenseSourceDirtyState.Active, Position.Of(TestMapLayers.One, 7, 9), 5));
            context.ItemEntityRegistry.AssignOrReplace(active10,
                                                       new SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>(0, ComputeDummySenseMap(10, new Position2D(26, 4))));
            context.ItemEntityRegistry.AssignOrReplace(active5,
                                                       new SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense>(0, ComputeDummySenseMap(5, new Position2D(7, 9))));
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomSenseMapA, EmptyRoomSenseMapB, EmptyRoomSenseMapAfterMove)]
        public void TestExpandDiscoveredArea(string id, string baseMap, string expectedSenseMapActorA, string expectedSenseMapActorB, string expectedSenseMapAfterMoveA)
        {
            SenseTestHelpers.ParseBool(baseMap, out var activeTestArea);
            var expectedMapActorA = SenseTestHelpers.ParseBool(expectedSenseMapActorA, out _);
            var expectedMapActorB = SenseTestHelpers.ParseBool(expectedSenseMapActorB, out _);
            var expectedMapActorAMoved = SenseTestHelpers.ParseBool(expectedSenseMapAfterMoveA, out _);

            var active10 = context.ItemResolver.Instantiate(senseReceptorActive10);
            var active5 = context.ItemResolver.Instantiate(senseReceptorActive5);

            PrepareReceptorItems(active10, active5);

            foreach (var s in this.senseSystemActions)
            {
                s();
            }

            context.ItemResolver.TryQueryData(active10, out IDiscoveryMap m1).Should().BeTrue();
            context.ItemResolver.TryQueryData(active5, out IDiscoveryMap m2).Should().BeTrue();

            m1.TryGetView(0, out var mapA).Should().BeTrue();
            m2.TryGetView(0, out var mapB).Should().BeTrue();

            Console.WriteLine("Computed Discovery Map Actor A (10):");
            Console.WriteLine(TestHelpers.PrintMap(mapA, activeTestArea));
            Console.WriteLine("--");
            Console.WriteLine("Computed Discovery Map Actor B (5):");
            Console.WriteLine(TestHelpers.PrintMap(mapB, activeTestArea));
            Console.WriteLine("--");

            TestHelpers.AssertEquals(mapA, expectedMapActorA, activeTestArea);
            TestHelpers.AssertEquals(mapB, expectedMapActorB, activeTestArea);

            // reposition the actor... 
            context.ItemPlacementService.TryMoveItem(active10, Position.Of(TestMapLayers.One, 26, 4), Position.Of(TestMapLayers.One, 14, 8)).Should().BeTrue();
            
            // .. update the sense map (that would be computed by the sense-receptor system ...
            context.ItemEntityRegistry.AssignOrReplace(active10, new SensoryReceptorState<VisionSense, TemperatureSense>(ComputeDummySourceData(5),
                                                                                                                         SenseSourceDirtyState.Active, Position.Of(TestMapLayers.One, 14, 8), 10));
            context.ItemEntityRegistry.AssignOrReplace(active10, new SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>(0, ComputeDummySenseMap(5, new Position2D(14, 8))));
            // .. and remap the discovered area.
            foreach (var s in this.senseSystemActions)
            {
                s();
            }

            Console.WriteLine("Computed Discovery Map Actor A (10) after move:");
            Console.WriteLine(TestHelpers.PrintMap(mapA, activeTestArea));
            Console.WriteLine("--");

            TestHelpers.AssertEquals(mapA, expectedMapActorAMoved, activeTestArea);
        }
    }
}
