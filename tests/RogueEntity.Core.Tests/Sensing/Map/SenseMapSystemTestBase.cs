using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
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
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Map
{
    public abstract class SenseMapSystemTestBase<TReceptorSense, TSourceSense, TSenseSourceDefinition>
        where TSourceSense: ISense
        where TReceptorSense: ISense
        where TSenseSourceDefinition : ISenseDefinition
    {

        ItemDeclarationId senseSourceActive10;
        ItemDeclarationId senseSourceActive5;
        ItemDeclarationId senseSourceInactive5;

        protected SenseMappingTestContext context;
        protected TestTimeSource timeSource;
        protected SensePropertiesSourceFixture<TSourceSense> senseProperties;
        protected SenseStateCache senseCache;
        List<Action<SenseMappingTestContext>> senseSystemActions;
        SenseMappingSystemBase<TReceptorSense, TSourceSense, TSenseSourceDefinition> senseSystem;

        [SetUp]
        public void SetUp()
        {
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
                                                                 .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                                 .DoWith(x => AttachTrait(x)));

            timeSource = new TestTimeSource();
            senseProperties = new SensePropertiesSourceFixture<TSourceSense>();
            senseCache = new SenseStateCache(2, 64, 64);

            senseSystem = CreateSystem();

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
            mapData.TryGetWritableView(0, out _, DataViewCreateMode.CreateMissing).Should().BeTrue();
        }
        
        protected abstract SensoryResistance<TSourceSense> Convert(float f);

        protected abstract ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl);

        protected abstract SenseMappingSystemBase<TReceptorSense, TSourceSense, TSenseSourceDefinition> CreateSystem();

        protected virtual List<Action<SenseMappingTestContext>> CreateSystemActions()
        {
            var builder = context.ItemEntityRegistry.BuildSystem().WithContext<SenseMappingTestContext>();
            var collectSystem = builder.CreateSystem<TSenseSourceDefinition, SenseSourceState<TSourceSense>>(senseSystem.CollectSenseSources);
            void ProcessAction(SenseMappingTestContext c) => senseSystem.ProcessSenseMap(c.ItemEntityRegistry);

            return new List<Action<SenseMappingTestContext>>
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
            context.ItemResolver.TryUpdateData(active10, context, EntityGridPosition.Of(TestMapLayers.One, 26, 7), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(active5, context, EntityGridPosition.Of(TestMapLayers.One, 8, 9), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(inactive, context, EntityGridPosition.Of(TestMapLayers.One, 11, 13), out _).Should().BeTrue();

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

        protected void PerformTest(string id, string sourceText, string expectedGlobalSenseMap)
        {
            senseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.Parse(sourceText, out var activeTestArea), Convert);

            var sourceActive10 = context.ItemResolver.Instantiate(context, senseSourceActive10);
            var sourceActive5 = context.ItemResolver.Instantiate(context, senseSourceActive5);
            var sourceInactive = context.ItemResolver.Instantiate(context, senseSourceInactive5);

            PrepareSourceItems(sourceActive10, sourceActive5, sourceInactive);

            foreach (var a in senseSystemActions)
            {
                a(context);
            }

            senseSystem.TryGetSenseData(0, out var globalSenseMap).Should().BeTrue();
            
            Console.WriteLine("Computed Global Sense Map:");
            Console.WriteLine(SenseTestHelpers.PrintMap(globalSenseMap, activeTestArea));
            Console.WriteLine("--");
            
            var expectedSenseMapData = SenseTestHelpers.Parse(expectedGlobalSenseMap, out _);
            SenseTestHelpers.AssertEquals(globalSenseMap, expectedSenseMapData, activeTestArea, new Position2D());
        }

        [TearDown]
        public void ShutDown()
        {
             senseSystem.ShutDown(context);
        }
        
    }
}