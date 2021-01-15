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
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Tests.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Sources
{
    static class SenseSystemTestBaseExtensions
    {
        internal static TObject DoWith<TObject>(this TObject o, Action<TObject> a)
        {
            a(o);
            return o;
        }
    }

    public abstract class SenseSystemTestBase<TSense, TSenseSourceDefinition>
        where TSense : ISense
        where TSenseSourceDefinition : ISenseDefinition
    {
        protected TestTimeSource timeSource;
        protected DynamicDataView3D<float> senseProperties;
        protected SenseStateCache senseCache;
        protected SenseMappingTestContext context;
        protected SenseSourceSystem<TSense, TSenseSourceDefinition> senseSystem;
        protected List<Action> senseSystemActions;
        protected ItemDeclarationId senseActive10;
        protected ItemDeclarationId senseActive5;
        protected ItemDeclarationId senseInactive5;
        protected SensoryResistanceDirectionalitySystem<TSense> directionalitySystem;
        protected virtual SensoryResistance<TSense> Convert(float f) => new SensoryResistance<TSense>(Percentage.Of(f));

        protected abstract ReferenceItemDeclaration<ItemReference> AttachTrait(ReferenceItemDeclaration<ItemReference> decl);

        protected virtual SenseSourceSystem<TSense, TSenseSourceDefinition> CreateSystem()
        {
            var physics = GetOrCreateSensePhysics();
            return new SenseSourceSystem<TSense, TSenseSourceDefinition>(senseProperties.AsLazy<IReadOnlyDynamicDataView3D<float>>(),
                                                                         senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                                                         timeSource.AsLazy<ITimeSource>(),
                                                                         directionalitySystem,
                                                                         senseCache,
                                                                         physics.Item1,
                                                                         physics.Item2);
        }

        protected abstract (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics();

        protected virtual List<Action> CreateSystemActions()
        {
            var ls = senseSystem;
            var builder = context.ItemEntityRegistry.BuildSystem()
                                 .WithoutContext();
            return new List<Action>
            {
                ls.BeginSenseCalculation,

                directionalitySystem.ProcessSystem,

                builder.WithInputParameter<TSenseSourceDefinition, EntityGridPosition>()
                       .WithOutputParameter<SenseSourceState<TSense>>()
                       .CreateSystem(ls.FindDirtySenseSources),
                Guard(builder.WithInputParameter<TSenseSourceDefinition, SenseDirtyFlag<TSense>, ObservedSenseSource<TSense>>()
                             .WithOutputParameter<SenseSourceState<TSense>>()
                             .CreateSystem(ls.RefreshLocalSenseState)),
                builder.WithInputParameter<TSenseSourceDefinition, SenseDirtyFlag<TSense>>()
                       .WithOutputParameter<SenseSourceState<TSense>>()
                       .CreateSystem(ls.ResetSenseSourceCacheState),

                ls.EndSenseCalculation
            };
        }

        [SetUp]
        public virtual void SetUp()
        {
            //lightPhysics = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));

            context = new SenseMappingTestContext();
            context.ItemEntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            context.ItemEntityRegistry.RegisterNonConstructable<TSenseSourceDefinition>();
            context.ItemEntityRegistry.RegisterNonConstructable<SenseSourceState<TSense>>();
            context.ItemEntityRegistry.RegisterFlag<ObservedSenseSource<TSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseDirtyFlag<TSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            context.ItemEntityRegistry.RegisterFlag<ImmobilityMarker>();

            senseActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseSource-Active-10")
                                                          .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                          .DoWith(x => AttachTrait(x)));
            senseActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseSource-Active-5")
                                                         .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                         .DoWith(x => AttachTrait(x)));
            senseInactive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>("SenseSource-Inactive-5")
                                                           .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(context.ItemResolver, context, TestMapLayers.One))
                                                           .DoWith(x => AttachTrait(x)));

            timeSource = new TestTimeSource();
            senseProperties = new DynamicDataView3D<float>();
            directionalitySystem = new SensoryResistanceDirectionalitySystem<TSense>(senseProperties);

            senseCache = new SenseStateCache(2, 64, 64);

            senseSystem = CreateSystem();
            senseSystem.EnsureSenseCacheAvailable();

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
            mapData.TryGetWritableView(0, out _, DataViewCreateMode.CreateMissing).Should().BeTrue();
        }

        [TearDown]
        public void ShutDown()
        {
            senseSystem.ShutDown();
        }

        protected static Action Guard(Action ac)
        {
            return () =>
            {
                // Keep on a single line for easy break point positioning.
                ac();
            };
        }

        protected virtual void PrepareItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemEntityRegistry.AssignComponent(active10, new ObservedSenseSource<TSense>());
            context.ItemEntityRegistry.AssignComponent(inactive, new ObservedSenseSource<TSense>());

            context.ItemResolver.TryUpdateData(active10, EntityGridPosition.Of(TestMapLayers.One, 3, 4), out _);
            context.ItemResolver.TryUpdateData(active5, EntityGridPosition.Of(TestMapLayers.One, 8, 9), out _);
            context.ItemResolver.TryUpdateData(inactive, EntityGridPosition.Of(TestMapLayers.One, 5, 5), out _);
        }

        protected void PerformTest(string id, string sourceText, string expectedResultText)
        {
            senseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.ParseMap(sourceText));

            var active10 = context.ItemResolver.Instantiate(senseActive10);
            var active5 = context.ItemResolver.Instantiate(senseActive5);
            var inactive = context.ItemResolver.Instantiate(senseInactive5);

            PrepareItems(active10, active5, inactive);

            foreach (var a in senseSystemActions)
            {
                a();
            }

            context.ItemEntityRegistry.GetComponent(active10, out SenseSourceState<TSense> va).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(active5, out SenseSourceState<TSense> vb).Should().BeTrue();
            bool haveInactiveState = context.ItemEntityRegistry.GetComponent(inactive, out SenseSourceState<TSense> vc);

            va.LastPosition.Should().Be(new Position(3, 4, 0, TestMapLayers.One));
            vb.LastPosition.Should().Be(new Position(8, 9, 0, TestMapLayers.One));

            va.State.Should().Be(SenseSourceDirtyState.Active);
            vb.State.Should().Be(SenseSourceDirtyState.Dirty, "Because (8, 9, 0) is not observed.");

            va.SenseSource.TryGetValue(out var vaData).Should().BeTrue("because this sense is observed");
            vb.SenseSource.TryGetValue(out _).Should().BeFalse("because this sense is not observed by anyone, and thus not computed");

            if (haveInactiveState)
            {
                vc.LastPosition.Should().Be(new Position());
                vc.State.Should().Be(SenseSourceDirtyState.Inactive);
                vc.SenseSource.TryGetValue(out _).Should().BeFalse("because this sense is inactive");
            }

            Console.WriteLine("Computed Result:");
            Console.WriteLine(TestHelpers.PrintMap(vaData, vaData.Bounds));
            Console.WriteLine("--");
            Console.WriteLine(TestHelpers.PrintMap(new SenseMapDirectionTestView(vaData), vaData.Bounds));
            var expectedResult = SenseTestHelpers.ParseMap(expectedResultText);

            // the resulting sense information is stored relative to the sense origin, with the origin point at the centre of the bounds
            // thus the result map must be mapped to the same area.
            TestHelpers.AssertEquals(vaData, expectedResult, expectedResult.GetActiveBounds(), new Position2D(vaData.Radius, vaData.Radius));
        }
    }
}
