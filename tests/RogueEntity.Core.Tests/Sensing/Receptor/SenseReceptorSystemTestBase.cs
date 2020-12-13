using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
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
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Receptor
{
    public abstract class SenseReceptorSystemBase<TReceptorSense, TSourceSense, TSenseSourceDefinition>
        where TReceptorSense : ISense
        where TSourceSense : ISense
        where TSenseSourceDefinition : ISenseDefinition
    {
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
        List<Action<SenseMappingTestContext>> senseSystemActions;

        protected SensoryResistance<TSourceSense> Convert(float f) => new SensoryResistance<TSourceSense>(f);

        protected abstract ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl);

        protected SensoryResistanceDirectionalitySystem<SenseMappingTestContext, TSourceSense> directionalitySourceSystem;
        protected SensoryResistanceDirectionalitySystem<SenseMappingTestContext, TReceptorSense> directionalityReceptorSystem;

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
            context.ItemEntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<SenseMappingTestContext, ItemReference>>();
            context.ItemEntityRegistry.RegisterNonConstructable<TSenseSourceDefinition>();
            context.ItemEntityRegistry.RegisterNonConstructable<SenseSourceState<TSourceSense>>();
            context.ItemEntityRegistry.RegisterFlag<ObservedSenseSource<TSourceSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseDirtyFlag<TSourceSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            context.ItemEntityRegistry.RegisterFlag<ImmobilityMarker>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorData<TReceptorSense, TSourceSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorState<TReceptorSense, TSourceSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>();

            senseSourceActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseSource-Active-10")
                                                                .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                                .DoWith(x => AttachTrait(x)));
            senseSourceActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseSource-Active-5")
                                                               .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                               .DoWith(x => AttachTrait(x)));
            senseSourceInactive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseSource-Inactive-5")
                                                                 .WithTrait(
                                                                     new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                                 .DoWith(x => AttachTrait(x)));

            senseReceptorActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseReceptor-Active-10")
                                                                  .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(
                                                                                 context.ItemResolver, context, TestMapLayers.One))
                                                                  .DoWith(x => AttachTrait(x)));
            senseReceptorActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseReceptor-Active-5")
                                                                 .WithTrait(
                                                                     new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                                 .DoWith(x => AttachTrait(x)));
            senseReceptorInactive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseReceptor-Inactive-5")
                                                                   .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(
                                                                                  context.ItemResolver, context, TestMapLayers.One))
                                                                   .DoWith(x => AttachTrait(x)));

            timeSource = new TestTimeSource();
            senseProperties = new DynamicDataView3D<float>();
            receptorSenseProperties = new DynamicDataView3D<float>();
            senseCache = new SenseStateCache(2, 64, 64);

            directionalityReceptorSystem = new SensoryResistanceDirectionalitySystem<SenseMappingTestContext, TReceptorSense>(receptorSenseProperties);
            directionalitySourceSystem = new SensoryResistanceDirectionalitySystem<SenseMappingTestContext, TSourceSense>(senseProperties);

            senseSystem = CreateSystem();
            senseSourceSystem = CreateSourceSystem();
            senseSystem.EnsureSenseCacheAvailable(context);

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
            mapData.TryGetWritableView(0, out _, DataViewCreateMode.CreateMissing).Should().BeTrue();
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

        protected virtual List<Action<SenseMappingTestContext>> CreateSystemActions()
        {
            var ls = senseSystem;
            var ss = senseSourceSystem;
            var builder = context.ItemEntityRegistry.BuildSystem().WithContext<SenseMappingTestContext>();
            return new List<Action<SenseMappingTestContext>>
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
                builder.WithInputParameter<TSenseSourceDefinition, SenseSourceState<TSourceSense>, EntityGridPosition>().CreateSystem(ss.FindDirtySenseSources), // 5500
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
                builder.WithInputParameter<TSenseSourceDefinition, SenseSourceState<TSourceSense>, SenseDirtyFlag<TSourceSense>>().CreateSystem(ss.ResetSenseSourceCacheState),
                ls.EndSenseCalculation,
                ss.EndSenseCalculation
            };
        }

        protected abstract Action<SenseMappingTestContext> CreateCopyAction();

        protected virtual void PrepareSourceItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemResolver.TryUpdateData(active10, context, EntityGridPosition.Of(TestMapLayers.One, 26, 7), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(active5, context, EntityGridPosition.Of(TestMapLayers.One, 8, 9), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(inactive, context, EntityGridPosition.Of(TestMapLayers.One, 11, 13), out _).Should().BeTrue();
        }

        protected virtual void PrepareReceptorItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemResolver.TryUpdateData(active10, context, EntityGridPosition.Of(TestMapLayers.One, 26, 4), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(active5, context, EntityGridPosition.Of(TestMapLayers.One, 7, 9), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(inactive, context, EntityGridPosition.Of(TestMapLayers.One, 13, 13), out _).Should().BeTrue();
        }

        protected void PerformTest(string id, string sourceText, string expectedPerceptionResult, string expectedSenseMap, string expectedSenseMapDirections)
        {
            senseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.ParseMap(sourceText, out var activeTestArea));
            receptorSenseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.ParseMap(sourceText, out _));

            var sourceActive10 = context.ItemResolver.Instantiate(context, senseSourceActive10);
            var sourceActive5 = context.ItemResolver.Instantiate(context, senseSourceActive5);
            var sourceInactive = context.ItemResolver.Instantiate(context, senseSourceInactive5);

            var active10 = context.ItemResolver.Instantiate(context, senseReceptorActive10);
            var active5 = context.ItemResolver.Instantiate(context, senseReceptorActive5);
            var inactive = context.ItemResolver.Instantiate(context, senseReceptorInactive5);

            PrepareSourceItems(sourceActive10, sourceActive5, sourceInactive);
            PrepareReceptorItems(active10, active5, inactive);

            foreach (var a in senseSystemActions)
            {
                a(context);
            }

            context.ItemEntityRegistry.GetComponent(active10, out SensoryReceptorState<TReceptorSense, TSourceSense> va).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(active5, out SensoryReceptorState<TReceptorSense, TSourceSense> vb).Should().BeTrue();
            bool haveInactiveState = context.ItemEntityRegistry.GetComponent(inactive, out SensoryReceptorState<TReceptorSense, TSourceSense> vc);

            va.LastPosition.Should().Be(new Position(26, 4, 0, TestMapLayers.One));
            vb.LastPosition.Should().Be(new Position(7, 9, 0, TestMapLayers.One));

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
                Console.WriteLine("Computed Sense Source:");
                Console.WriteLine(TestHelpers.PrintMap(sourceData.TranslateBy<float>(26, 7), activeTestArea));
                Console.WriteLine("--> Directions:");
                Console.WriteLine(TestHelpers.PrintMap(new SenseMapDirectionTestView(sourceData).TranslateBy(26, 7), activeTestArea));
            }

            Console.WriteLine("Computed Perception Result:");
            Console.WriteLine(TestHelpers.PrintMap(vaData.TranslateBy<float>(26, 4), activeTestArea));
            Console.WriteLine("--> Directions:");
            Console.WriteLine(TestHelpers.PrintMap(new SenseMapDirectionTestView(vaData).TranslateBy(26, 4), activeTestArea));
            Console.WriteLine("--");

            context.ItemEntityRegistry.GetComponent(active10, out SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> rawSenseData).Should().BeTrue();
            rawSenseData.TryGetIntensity(0, out var senseData).Should().BeTrue();
            senseData.GetActiveBounds().Width.Should().NotBe(0);
            senseData.GetActiveBounds().Height.Should().NotBe(0);

            Console.WriteLine("Computed SenseMap Result:");
            Console.WriteLine(TestHelpers.PrintMap(senseData, activeTestArea));
            Console.WriteLine("--");
            Console.WriteLine(TestHelpers.PrintMap(new SenseMapDirectionTestView(senseData), activeTestArea));

            // the resulting sense information is stored relative to the sense origin, with the origin point at the centre of the bounds
            // thus the result map must be mapped to the same area.
            var expectedPerceptionData = SenseTestHelpers.ParseMap(expectedPerceptionResult, out _);
            TestHelpers.AssertEquals(vaData, expectedPerceptionData, activeTestArea, new Position2D(26, 4));

            var expectedSenseMapData = SenseTestHelpers.ParseMap(expectedSenseMap, out _);
            TestHelpers.AssertEquals(senseData, expectedSenseMapData, activeTestArea, new Position2D());

            var expectedSenseMapDirectionData = SenseTestHelpers.ParseDirections(expectedSenseMapDirections, out _);
            TestHelpers.AssertEquals(senseData, expectedSenseMapDirectionData, activeTestArea, new Position2D(), SenseTestHelpers.PrintSenseDirectionStore);
        }

        [TearDown]
        public void ShutDown()
        {
            // senseSystem.ShutDown(context);
        }
    }
}
