using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Tests.Sensing.Common;
using RogueEntity.Core.Tests.Sensing.Sources;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using Serilog;
using Serilog.Events;

namespace RogueEntity.Core.Tests.Sensing.Receptor
{
    public abstract class SenseReceptorSystemBase<TReceptorSense, TSourceSense, TSenseSourceDefinition>
        where TReceptorSense : ISense
        where TSourceSense : ISense
        where TSenseSourceDefinition : ISenseDefinition
    {
        protected readonly ILogger Logger;
        protected SenseMappingTestContext context;
        protected SenseReceptorSystem<TReceptorSense, TSourceSense> senseSystem;
        protected SenseSourceSystem<TSourceSense, TSenseSourceDefinition> senseSourceSystem;

        ItemDeclarationId senseSourceActive10;
        ItemDeclarationId senseSourceActive5;
        ItemDeclarationId senseSourceInactive5;

        ItemDeclarationId senseReceptorActive10;
        ItemDeclarationId senseReceptorActive5;
        ItemDeclarationId senseReceptorInactive5;

        protected TestTimeSource timeSource;
        protected DynamicDataView3D<float> senseProperties;
        protected DynamicDataView3D<float> receptorSenseProperties;
        protected SenseStateCache senseCache;
        List<Action> senseSystemActions;

        protected SensoryResistance<TSourceSense> Convert(float f) => new SensoryResistance<TSourceSense>(f);

        protected abstract IReferenceItemDeclaration<ItemReference> AttachTrait(IReferenceItemDeclaration<ItemReference> decl);

        protected SensoryResistanceDirectionalitySystem<TSourceSense> directionalitySourceSystem;
        protected SensoryResistanceDirectionalitySystem<TReceptorSense> directionalityReceptorSystem;

        public SenseReceptorSystemBase()
        {
            Logger = SLog.ForContext(GetType());
        }

        protected abstract (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateReceptorSensePhysics();

        protected virtual SenseReceptorSystem<TReceptorSense, TSourceSense> CreateSystem()
        {
            var physics = GetOrCreateReceptorSensePhysics();
            return new SenseReceptorSystem<TReceptorSense, TSourceSense>(receptorSenseProperties.AsLazy<IReadOnlyDynamicDataView3D<float>>(),
                                                                         senseCache.AsLazy<ISenseStateCacheProvider>(),
                                                                         senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                                                         timeSource.AsLazy<ITimeSource>(),
                                                                         directionalityReceptorSystem,
                                                                         physics.Item2,
                                                                         physics.Item1);
        }

        [SetUp]
        public virtual void SetUp()
        {
            //lightPhysics = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));

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
                                                                 .WithTrait(
                                                                     new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, TestMapLayers.One))
                                                                 .DoWith(x => AttachTrait(x)));

            senseReceptorActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseReceptor-Active-10")
                                                                  .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, TestMapLayers.One))
                                                                  .DoWith(x => AttachTrait(x)));
            senseReceptorActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseReceptor-Active-5")
                                                                 .WithTrait(
                                                                     new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, TestMapLayers.One))
                                                                 .DoWith(x => AttachTrait(x)));
            senseReceptorInactive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseReceptor-Inactive-5")
                                                                   .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, TestMapLayers.One))
                                                                   .DoWith(x => AttachTrait(x)));

            timeSource = new TestTimeSource(new RealTimeSourceDefinition(30));
            senseProperties = new DynamicDataView3D<float>();
            receptorSenseProperties = new DynamicDataView3D<float>();
            senseCache = new SenseStateCache(2, 64, 64);

            directionalityReceptorSystem = new SensoryResistanceDirectionalitySystem<TReceptorSense>(receptorSenseProperties);
            directionalitySourceSystem = new SensoryResistanceDirectionalitySystem<TSourceSense>(senseProperties);

            senseSystem = CreateSystem();
            senseSourceSystem = CreateSourceSystem();
            senseSystem.EnsureSenseCacheAvailable();

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
        }

        protected abstract (ISensePropagationAlgorithm propagationAlgorithm, ISensePhysics sensePhysics) GetOrCreateSourceSensePhysics();

        protected virtual SenseSourceSystem<TSourceSense, TSenseSourceDefinition> CreateSourceSystem()
        {
            var physics = GetOrCreateSourceSensePhysics();
            return new SenseSourceSystem<TSourceSense, TSenseSourceDefinition>(senseProperties.AsLazy<IReadOnlyDynamicDataView3D<float>>(),
                                                                               senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                                                               timeSource.AsLazy<ITimeSource>(),
                                                                               directionalitySourceSystem,
                                                                               senseCache,
                                                                               physics.propagationAlgorithm,
                                                                               physics.sensePhysics);
        }

        protected virtual List<Action> CreateSystemActions()
        {
            var ls = senseSystem;
            var ss = senseSourceSystem;
            var builder = context.ItemEntityRegistry.BuildSystem()
                                 .WithoutContext();
            return new List<Action>
            {
                ls.EnsureSenseCacheAvailable,
                ss.EnsureSenseCacheAvailable,

                ls.BeginSenseCalculation,
                ss.BeginSenseCalculation,

                directionalityReceptorSystem.ProcessSystem,
                directionalitySourceSystem.ProcessSystem,

                builder.WithInputParameter<SensoryReceptorData<TReceptorSense, TSourceSense>,
                           EntityGridPosition>()
                       .WithOutputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                       .CreateSystem(ls.CollectReceptor), // 5550
                builder.WithInputParameter<TSenseSourceDefinition, EntityGridPosition>()
                       .WithOutputParameter<SenseSourceState<TSourceSense>>()
                       .CreateSystem(ss.FindDirtySenseSources), // 5500
                builder.WithInputParameter<SensoryReceptorData<TReceptorSense, TSourceSense>,
                           SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>()
                       .WithOutputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                       .CreateSystem(ls.RefreshLocalReceptorState), // 5600
                builder.WithInputParameter<TSenseSourceDefinition, SenseSourceState<TSourceSense>>().CreateSystem(ls.CollectObservedSenseSource), // 5750
                builder.WithInputParameter<TSenseSourceDefinition, SenseDirtyFlag<TSourceSense>, ObservedSenseSource<TSourceSense>>()
                       .WithOutputParameter<SenseSourceState<TSourceSense>>()
                       .CreateSystem(ss.RefreshLocalSenseState), // 5800
                CreateCopyAction(), // 5850
                builder
                    .WithInputParameter<SensoryReceptorData<TReceptorSense, TSourceSense>, SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>()
                    .WithOutputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                    .CreateSystem(ls.ResetReceptorCacheState), // 5900
                builder.WithInputParameter<ObservedSenseSource<TSourceSense>>().CreateSystem(ls.ResetSenseSourceObservedState), // 5900
                builder.WithInputParameter<TSenseSourceDefinition, SenseDirtyFlag<TSourceSense>>()
                       .WithOutputParameter<SenseSourceState<TSourceSense>>()
                       .CreateSystem(ss.ResetSenseSourceCacheState),
                ls.EndSenseCalculation,
                ss.EndSenseCalculation
            };
        }

        protected abstract Action CreateCopyAction();

        protected virtual void PrepareSourceItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemPlacementService.TryPlaceItem(active10, Position.Of(TestMapLayers.One, 26, 7)).Should().BeTrue();
            context.ItemPlacementService.TryPlaceItem(active5, Position.Of(TestMapLayers.One, 8, 9)).Should().BeTrue();
            context.ItemPlacementService.TryPlaceItem(inactive, Position.Of(TestMapLayers.One, 11, 13)).Should().BeTrue();
        }

        protected virtual void PrepareReceptorItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemPlacementService.TryPlaceItem(active10, Position.Of(TestMapLayers.One, 26, 4)).Should().BeTrue();
            context.ItemPlacementService.TryPlaceItem(active5, Position.Of(TestMapLayers.One, 7, 9)).Should().BeTrue();
            context.ItemPlacementService.TryPlaceItem(inactive, Position.Of(TestMapLayers.One, 13, 13)).Should().BeTrue();
        }

        public readonly struct TestData
        {
            public readonly string SourceText;
            public readonly string ExpectedPerceptionResult;
            public readonly string ExpectedSenseMap;
            public readonly string ExpectedSenseMapDirections;

            public TestData(string sourceText, string expectedPerceptionResult, string expectedSenseMap, string expectedSenseMapDirections)
            {
                SourceText = sourceText;
                ExpectedPerceptionResult = expectedPerceptionResult;
                ExpectedSenseMap = expectedSenseMap;
                ExpectedSenseMapDirections = expectedSenseMapDirections;
            }

            public void Deconstruct(out string sourceText, out string expectedPerceptionResult, out string expectedSenseMap, out string expectedSenseMapDirections)
            {
                sourceText = SourceText;
                expectedPerceptionResult = ExpectedPerceptionResult;
                expectedSenseMap = ExpectedSenseMap;
                expectedSenseMapDirections = ExpectedSenseMapDirections;
            }
        }

        protected abstract TestData FetchTestData(string id);
        
        protected void PerformTest(string id)
        {
            var (sourceText, expectedPerceptionResult, expectedSenseMap, expectedSenseMapDirections) = FetchTestData(id);
            
            senseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.ParseMap(sourceText, out var activeTestArea));
            receptorSenseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.ParseMap(sourceText, out _));

            var sourceActive10 = context.ItemResolver.Instantiate(senseSourceActive10);
            var sourceActive5 = context.ItemResolver.Instantiate(senseSourceActive5);
            var sourceInactive = context.ItemResolver.Instantiate(senseSourceInactive5);

            var active10 = context.ItemResolver.Instantiate(senseReceptorActive10);
            var active5 = context.ItemResolver.Instantiate(senseReceptorActive5);
            var inactive = context.ItemResolver.Instantiate(senseReceptorInactive5);

            PrepareSourceItems(sourceActive10, sourceActive5, sourceInactive);
            PrepareReceptorItems(active10, active5, inactive);

            foreach (var a in senseSystemActions)
            {
                a();
            }

            context.ItemEntityRegistry.GetComponent(active10, out SensoryReceptorState<TReceptorSense, TSourceSense> va).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(active5, out SensoryReceptorState<TReceptorSense, TSourceSense> vb).Should().BeTrue();
            bool haveInactiveState = context.ItemEntityRegistry.GetComponent(inactive, out SensoryReceptorState<TReceptorSense, TSourceSense> vc);

            va.LastPosition.Should().Be(Position.Of(TestMapLayers.One, 26, 4));
            vb.LastPosition.Should().Be(Position.Of(TestMapLayers.One, 7, 9));

            va.State.Should().Be(SenseSourceDirtyState.Active);
            vb.State.Should().Be(SenseSourceDirtyState.Active);

            va.SenseSource.TryGetValue(out var vaData).Should().BeTrue();
            vb.SenseSource.TryGetValue(out _).Should().BeTrue();

            if (haveInactiveState)
            {
                vc.LastPosition.Should().Be(new Position());
                vc.State.Should().Be(SenseSourceDirtyState.Inactive);
                vc.SenseSource.TryGetValue(out _).Should().BeFalse("because this sense is inactive");
            }

            if (context.ItemEntityRegistry.GetComponent(sourceActive10, out SenseSourceState<TSourceSense> sourceState) ||
                context.ItemEntityRegistry.GetComponent(active10, out sourceState))
            {
                sourceState.SenseSource.TryGetValue(out var sourceData).Should().BeTrue();
                if (Logger.IsEnabled(LogEventLevel.Debug))
                {
                    Logger.Debug("Computed Sense Source:\n{SourceData}\n-->Directions\n{DirectionData}",
                                 TestHelpers.PrintMap(sourceData.TranslateBy<float>(26, 7), activeTestArea),
                                 TestHelpers.PrintMap(new SenseMapDirectionTestView(sourceData).TranslateBy(26, 7), activeTestArea));
                }
            }

            if (Logger.IsEnabled(LogEventLevel.Debug))
            {
                Logger.Debug("Computed Perception Result:\n{Data}\n{Directions}",
                             TestHelpers.PrintMap(vaData.TranslateBy<float>(26, 4), activeTestArea),
                             TestHelpers.PrintMap(new SenseMapDirectionTestView(vaData).TranslateBy(26, 4), activeTestArea));
            }

            context.ItemEntityRegistry.GetComponent(active10, out SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> rawSenseData).Should().BeTrue();
            rawSenseData.TryGetIntensity(0, out var senseData).Should().BeTrue();
            senseData.GetActiveBounds().Width.Should().NotBe(0);
            senseData.GetActiveBounds().Height.Should().NotBe(0);
            if (Logger.IsEnabled(LogEventLevel.Debug))
            {
                Logger.Debug("Computed SenseMap Result:\n{Data}\n{Directions}",
                             TestHelpers.PrintMap(senseData, activeTestArea),
                             TestHelpers.PrintMap(new SenseMapDirectionTestView(senseData), activeTestArea));
            }

            // the resulting sense information is stored relative to the sense origin, with the origin point at the centre of the bounds
            // thus the result map must be mapped to the same area.
            var expectedPerceptionData = SenseTestHelpers.ParseMap(expectedPerceptionResult, out _);
            TestHelpers.AssertEquals(vaData, expectedPerceptionData, activeTestArea, new GridPosition2D(26, 4));

            var expectedSenseMapData = SenseTestHelpers.ParseMap(expectedSenseMap, out _);
            TestHelpers.AssertEquals(senseData, expectedSenseMapData, activeTestArea, new GridPosition2D());

            var expectedSenseMapDirectionData = SenseTestHelpers.ParseDirections(expectedSenseMapDirections, out _);
            TestHelpers.AssertEquals(senseData, expectedSenseMapDirectionData, activeTestArea, new GridPosition2D(), SenseTestHelpers.PrintSenseDirectionStore);
        }

        [TearDown]
        public void ShutDown()
        {
            // senseSystem.ShutDown(context);
        }
    }
}
