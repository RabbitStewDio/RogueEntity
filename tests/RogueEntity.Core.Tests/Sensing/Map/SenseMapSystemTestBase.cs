using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Map;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Tests.Sensing.Sources;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Sensing.Map
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public abstract class SenseMapSystemTestBase<TReceptorSense, TSourceSense, TSenseSourceDefinition>
        where TSourceSense : ISense
        where TReceptorSense : ISense
        where TSenseSourceDefinition : ISenseDefinition
    {
        ItemDeclarationId senseSourceActive10;
        ItemDeclarationId senseSourceActive5;
        ItemDeclarationId senseSourceInactive5;

        protected SenseMappingTestContext context;
        protected TestTimeSource timeSource;
        protected DynamicDataView3D<float> senseProperties;
        protected SenseStateCache senseCache;
        List<Action> senseSystemActions;
        SenseMappingSystemBase<TReceptorSense, TSourceSense, TSenseSourceDefinition> senseSystem;

        [SetUp]
        public void SetUp()
        {
            context = new SenseMappingTestContext();
            context.ItemEntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            context.ItemEntityRegistry.RegisterNonConstructable<TSenseSourceDefinition>();
            context.ItemEntityRegistry.RegisterNonConstructable<SenseSourceState<TSourceSense>>();
            context.ItemEntityRegistry.RegisterFlag<ObservedSenseSource<TSourceSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseDirtyFlag<TSourceSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            context.ItemEntityRegistry.RegisterFlag<ImmobilityMarker>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorData<TReceptorSense, TSourceSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorState<TReceptorSense, TSourceSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>();

            senseSourceActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseSource-Active-10")
                                                                .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, TestMapLayers.One))
                                                                .DoWith(x => AttachTrait(x)));
            senseSourceActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseSource-Active-5")
                                                               .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, TestMapLayers.One))
                                                               .DoWith(x => AttachTrait(x)));
            senseSourceInactive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseSource-Inactive-5")
                                                                 .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, TestMapLayers.One))
                                                                 .DoWith(x => AttachTrait(x)));

            timeSource = new TestTimeSource(new RealTimeSourceDefinition(30));
            senseProperties = new DynamicDataView3D<float>();
            senseCache = new SenseStateCache(2, 64, 64);

            senseSystem = CreateSystem();

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
        }

        protected abstract SensoryResistance<TSourceSense> Convert(float f);

        protected abstract IReferenceItemDeclaration<ItemReference> AttachTrait(IReferenceItemDeclaration<ItemReference> decl);

        protected abstract SenseMappingSystemBase<TReceptorSense, TSourceSense, TSenseSourceDefinition> CreateSystem();

        protected virtual List<Action> CreateSystemActions()
        {
            var builder = context.ItemEntityRegistry.BuildSystem().WithoutContext();
            var collectSystem = builder.WithInputParameter<TSenseSourceDefinition, SenseSourceState<TSourceSense>>().CreateSystem(senseSystem.CollectSenseSources);
            void ProcessAction() => senseSystem.ProcessSenseMap(context.ItemEntityRegistry);

            return new List<Action>
            {
                collectSystem,
                ProcessAction,
                senseSystem.EndSenseCalculation
            };
        }

        /// <summary>
        ///   The source's sense emission field is calculated in another system. We'll be using precomputed values instead.
        /// </summary>
        /// <param name="active10"></param>
        /// <param name="active5"></param>
        /// <param name="inactive"></param>
        protected virtual void PrepareSourceItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemPlacementService.TryPlaceItem(active10, Position.Of(TestMapLayers.One, 26, 7)).Should().BeTrue();
            context.ItemPlacementService.TryPlaceItem(active5, Position.Of(TestMapLayers.One, 8, 9)).Should().BeTrue();
            context.ItemPlacementService.TryPlaceItem(inactive, Position.Of(TestMapLayers.One, 11, 13)).Should().BeTrue();

            context.ItemEntityRegistry.AssignOrReplace(active10,
                                                       new SenseSourceState<TSourceSense>(ComputeDummySourceData(active10, 10),
                                                                                          SenseSourceDirtyState.Active, Position.Of(TestMapLayers.One, 26, 7)));
            context.ItemEntityRegistry.AssignOrReplace(active5,
                                                       new SenseSourceState<TSourceSense>(ComputeDummySourceData(active5, 5),
                                                                                          SenseSourceDirtyState.Active, Position.Of(TestMapLayers.One, 8, 9)));
            context.ItemEntityRegistry.AssignOrReplace(inactive,
                                                       new SenseSourceState<TSourceSense>(default,
                                                                                          SenseSourceDirtyState.Inactive, Position.Invalid));
        }

        protected SenseSourceData ComputeDummySourceData(ItemReference e, int radius)
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

        protected void PerformTest(string id, string sourceText, string expectedGlobalSenseMap)
        {
            senseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.ParseMap(sourceText, out var activeTestArea));

            var sourceActive10 = context.ItemResolver.Instantiate(senseSourceActive10);
            var sourceActive5 = context.ItemResolver.Instantiate(senseSourceActive5);
            var sourceInactive = context.ItemResolver.Instantiate(senseSourceInactive5);

            PrepareSourceItems(sourceActive10, sourceActive5, sourceInactive);

            foreach (var a in senseSystemActions)
            {
                a();
            }

            senseSystem.TryGetSenseData(0, out var globalSenseMap).Should().BeTrue();

            Console.WriteLine("Computed Global Sense Map:");
            Console.WriteLine(TestHelpers.PrintMap(globalSenseMap, activeTestArea));
            Console.WriteLine("--");

            var expectedSenseMapData = SenseTestHelpers.ParseMap(expectedGlobalSenseMap, out _);
            TestHelpers.AssertEquals(globalSenseMap, expectedSenseMapData, activeTestArea, new Position2D());
        }

        [TearDown]
        public void ShutDown()
        {
            senseSystem.ShutDown();
        }
    }
}
