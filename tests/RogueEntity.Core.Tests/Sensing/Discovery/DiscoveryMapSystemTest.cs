using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
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
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Sensing.Discovery
{
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
        List<Action<SenseMappingTestContext>> senseSystemActions;
        DiscoveryMapSystem senseSystem;


        [SetUp]
        public void SetUp()
        {
            context = new SenseMappingTestContext();
            context.ItemEntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<SenseMappingTestContext, ItemReference>>();
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

            var visionSensePhysics = new InfraVisionSenseReceptorPhysicsConfiguration(new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0)));
            var noiseSensePhysics =
                new NoiseSenseReceptorPhysicsConfiguration(new NoisePhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)), new FloodFillWorkingDataSource());

            senseReceptorActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseReceptor-Active-10")
                                                                  .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, TestMapLayers.One))
                                                                  .WithTrait(new DiscoveryMapTrait<SenseMappingTestContext, ItemReference>())
                                                                  .WithTrait(new InfraVisionSenseTrait<SenseMappingTestContext, ItemReference>(visionSensePhysics, 10))
            );
            senseReceptorActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseReceptor-Active-5")
                                                                 .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, TestMapLayers.One))
                                                                 .WithTrait(new DiscoveryMapTrait<SenseMappingTestContext, ItemReference>())
                                                                 .WithTrait(new NoiseDirectionSenseTrait<SenseMappingTestContext, ItemReference>(noiseSensePhysics, 10))
            );

            senseSystem = new DiscoveryMapSystem();

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
            mapData.TryGetWritableView(0, out _, DataViewCreateMode.CreateMissing).Should().BeTrue();
        }

        protected virtual List<Action<SenseMappingTestContext>> CreateSystemActions()
        {
            var builder = context.ItemEntityRegistry.BuildSystem().WithContext<SenseMappingTestContext>();

            return new List<Action<SenseMappingTestContext>>
            {
                builder.CreateSystem<DiscoveryMapData,
                    SensoryReceptorState<VisionSense, TemperatureSense>,
                    SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>,
                    SenseReceptorDirtyFlag<VisionSense, TemperatureSense>>(senseSystem.ExpandDiscoveredArea),
                
                builder.CreateSystem<DiscoveryMapData,
                    SensoryReceptorState<NoiseSense, NoiseSense>,
                    SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense>,
                    SenseReceptorDirtyFlag<NoiseSense, NoiseSense>>(senseSystem.ExpandDiscoveredArea)
            };
        }

        protected SenseSourceData ComputeDummySourceData(int radius)
        {
            var sd = new SenseSourceData(10);
            foreach (var p in sd.Bounds.Contents)
            {
                var str = radius - (float)DistanceCalculation.Euclid.Calculate(p);
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
                var str = radius - (float)DistanceCalculation.Euclid.Calculate(p - origin);
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
            context.ItemResolver.TryUpdateData(active10, context, EntityGridPosition.Of(TestMapLayers.One, 26, 4), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(active5, context, EntityGridPosition.Of(TestMapLayers.One, 7, 9), out _).Should().BeTrue();

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
            
            var active10 = context.ItemResolver.Instantiate(context, senseReceptorActive10);
            var active5 = context.ItemResolver.Instantiate(context, senseReceptorActive5);

            PrepareReceptorItems(active10, active5);

            foreach (var s in this.senseSystemActions)
            {
                s(context);
            }

            context.ItemResolver.TryQueryData(active10, context, out IDiscoveryMap m1).Should().BeTrue();
            context.ItemResolver.TryQueryData(active5, context, out IDiscoveryMap m2).Should().BeTrue();

            m1.TryGetMap(0, out var mapA).Should().BeTrue();
            m2.TryGetMap(0, out var mapB).Should().BeTrue();
            
            Console.WriteLine("Computed Discovery Map Actor A (10):");
            Console.WriteLine(SenseTestHelpers.PrintMap(mapA, activeTestArea));
            Console.WriteLine("--");
            Console.WriteLine("Computed Discovery Map Actor B (5):");
            Console.WriteLine(SenseTestHelpers.PrintMap(mapB, activeTestArea));
            Console.WriteLine("--");

            SenseTestHelpers.AssertEquals(mapA, expectedMapActorA, activeTestArea, new Position2D());
            SenseTestHelpers.AssertEquals(mapB, expectedMapActorB, activeTestArea, new Position2D());

            // reposition the actor... 
            context.ItemResolver.TryUpdateData(active10, context, EntityGridPosition.Of(TestMapLayers.One, 14, 8), out _).Should().BeTrue();
            // .. update the sense map (that would be computed by the sense-receptor system ...
            context.ItemEntityRegistry.AssignOrReplace(active10, new SensoryReceptorState<VisionSense, TemperatureSense>(ComputeDummySourceData(5),
                                                                                                                         SenseSourceDirtyState.Active, Position.Of(TestMapLayers.One, 14, 8), 10));
            context.ItemEntityRegistry.AssignOrReplace(active10, new SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>(0, ComputeDummySenseMap(5, new Position2D(14, 8))));
            // .. and remap the discovered area.
            foreach (var s in this.senseSystemActions)
            {
                s(context);
            }
            
            Console.WriteLine("Computed Discovery Map Actor A (10) after move:");
            Console.WriteLine(SenseTestHelpers.PrintMap(mapA, activeTestArea));
            Console.WriteLine("--");
            
            SenseTestHelpers.AssertEquals(mapA, expectedMapActorAMoved, activeTestArea, new Position2D());
            
        }
    }
}