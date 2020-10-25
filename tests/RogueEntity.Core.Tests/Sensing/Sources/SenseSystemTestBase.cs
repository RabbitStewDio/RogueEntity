using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;

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
    
    public abstract class SenseSystemTestBase<TSense, 
                                              TSenseSystem, 
                                              TSenseSourceDefinition>
        where TSense : ISense
        where TSenseSystem : SenseSystemBase<TSense, TSenseSourceDefinition>
        where TSenseSourceDefinition : ISenseDefinition
    {
        protected TestTimeSource timeSource;
        protected SensePropertiesSourceFixture senseProperties;
        protected SenseStateCache senseCache;
        protected SenseMappingTestContext context;
        protected TSenseSystem senseSystem;
        protected List<Action<SenseMappingTestContext>> senseSystemActions;
        protected ItemDeclarationId senseActive10;
        protected ItemDeclarationId senseActive5;
        protected ItemDeclarationId senseInactive5;

        protected abstract SensoryResistance Convert(float f);

        protected abstract ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl);

        protected abstract TSenseSystem CreateSystem();


        protected virtual List<Action<SenseMappingTestContext>> CreateSystemActions()
        {
            var ls = senseSystem;
            var builder = context.ItemEntityRegistry.BuildSystem()
                                 .WithContext<SenseMappingTestContext>();
            return new List<Action<SenseMappingTestContext>>
            {
                ls.BeginSenseCalculation,

                builder.CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, EntityGridPosition>(ls.FindDirtySenseSources),
                Guard(builder.CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, SenseDirtyFlag<TSense>, ObservedSenseSource<TSense>>(ls.RefreshLocalSenseState)),
                builder.CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, SenseDirtyFlag<TSense>>(ls.ResetSenseSourceCacheState),

                ls.EndSenseCalculation
            };
        }
        
        [SetUp]
        public virtual void SetUp()
        {
            //lightPhysics = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));

            context = new SenseMappingTestContext();
            context.ItemEntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<SenseMappingTestContext, ItemReference>>();
            context.ItemEntityRegistry.RegisterNonConstructable<TSenseSourceDefinition>();
            context.ItemEntityRegistry.RegisterNonConstructable<SenseSourceState<TSense>>();
            context.ItemEntityRegistry.RegisterFlag<ObservedSenseSource<TSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseDirtyFlag<TSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            context.ItemEntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            context.ItemEntityRegistry.RegisterFlag<ImmobilityMarker>();

            senseActive10 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseSource-Active-10")
                                                          .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, TestMapLayers.One))
                                                          .DoWith(x => AttachTrait(x)));
            senseActive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseSource-Active-5")
                                                         .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, TestMapLayers.One))
                                                         .DoWith(x => AttachTrait(x)));
            senseInactive5 = context.ItemRegistry.Register(new ReferenceItemDeclaration<SenseMappingTestContext, ItemReference>("SenseSource-Inactive-5")
                                                           .WithTrait(new ReferenceItemGridPositionTrait<SenseMappingTestContext, ItemReference>(context.ItemResolver, TestMapLayers.One))
                                                           .DoWith(x => AttachTrait(x)));

            timeSource = new TestTimeSource();

            senseProperties = new SensePropertiesSourceFixture();

            senseCache = new SenseStateCache(2, 64, 64);

            senseSystem = CreateSystem();
            senseSystem.EnsureSenseCacheAvailable(context);

            senseSystemActions = CreateSystemActions();

            context.TryGetItemGridDataFor(TestMapLayers.One, out var mapData).Should().BeTrue();
            mapData.TryGetMap(0, out var map, MapAccess.ForWriting).Should().BeTrue();
        }

        [TearDown]
        public void ShutDown()
        {
            senseSystem.ShutDown(context);
        }

        protected static Action<SenseMappingTestContext> Guard(Action<SenseMappingTestContext> ac)
        {
            return ctx =>
            {
                ac(ctx);
            };
        }

        protected virtual void PrepareItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemEntityRegistry.AssignComponent(active10, new ObservedSenseSource<TSense>());
            context.ItemEntityRegistry.AssignComponent(inactive, new ObservedSenseSource<TSense>());

            context.ItemResolver.TryUpdateData(active10, context, EntityGridPosition.Of(TestMapLayers.One, 3, 4), out _);
            context.ItemResolver.TryUpdateData(active5, context, EntityGridPosition.Of(TestMapLayers.One, 8, 9), out _);
            context.ItemResolver.TryUpdateData(inactive, context, EntityGridPosition.Of(TestMapLayers.One, 5, 5), out _);
        }
        
        protected void PerformTest(string id, string sourceText, string expectedResultText)
        {
            senseProperties.GetOrCreate(0).ImportData(SenseTestHelpers.Parse(sourceText), Convert);

            var active10 = context.ItemResolver.Instantiate(context, senseActive10);
            var active5 = context.ItemResolver.Instantiate(context, senseActive5);
            var inactive = context.ItemResolver.Instantiate(context, senseInactive5);
            
            PrepareItems(active10, active5, inactive);
            
            foreach (var a in senseSystemActions)
            {
                a(context);
            }

            context.ItemEntityRegistry.GetComponent(active10, out SenseSourceState<TSense> va).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(active5, out SenseSourceState<TSense> vb).Should().BeTrue();
            bool haveInactiveState = context.ItemEntityRegistry.GetComponent(inactive, out SenseSourceState<TSense> vc);

            va.LastPosition.Should().Be(new Position(3, 4, 0, TestMapLayers.One));
            vb.LastPosition.Should().Be(new Position(8, 9, 0, TestMapLayers.One));

            va.State.Should().Be(SenseSourceDirtyState.Active);
            vb.State.Should().Be(SenseSourceDirtyState.Active);

            va.SenseSource.TryGetValue(out var vaData).Should().BeTrue("because this sense is observed");
            vb.SenseSource.TryGetValue(out var vbData).Should().BeFalse("because this sense is not observed by anyone, and thus not computed");

            if (haveInactiveState)
            {
                vc.LastPosition.Should().Be(new Position());
                vc.State.Should().Be(SenseSourceDirtyState.Inactive);
                vc.SenseSource.TryGetValue(out var vcData).Should().BeFalse("because this sense is inactive");
            }

            Console.WriteLine("Computed Result:");
            Console.WriteLine(SenseTestHelpers.PrintMap(vaData, vaData.Bounds));
            Console.WriteLine("--");
            var expectedResult = SenseTestHelpers.Parse(expectedResultText);

            // the resulting sense information is stored relative to the sense origin, with the origin point at the centre of the bounds
            // thus the result map must be mapped to the same area.
            SenseTestHelpers.AssertEquals(vaData, expectedResult, expectedResult.ActiveBounds, new Position2D(vaData.Radius, vaData.Radius));
        }
    }
}