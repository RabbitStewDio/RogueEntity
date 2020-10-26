using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Touch;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Touch
{
    public class TouchSystemTest : SenseSystemTestBase<TouchSense, TouchSystem, TouchSourceDefinition>
    {
        const string EmptyRoom = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
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

        const string EmptyRoomResult = @"
   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  1.000,  1.000,   .   
   .   ,  1.000,  2.000,  1.000,   .   
   .   ,  1.000,  1.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   . ";

        TouchPhysicsConfiguration lightPhysics;

        protected override SensoryResistance Convert(float f)
        {
            return new SensoryResistance(Percentage.Empty, Percentage.Of(f), Percentage.Empty, Percentage.Empty);
        }

        protected override TouchSystem CreateSystem()
        {
            return new TouchSystem(senseProperties.AsLazy<ISensePropertiesSource>(),
                                   senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                   timeSource.AsLazy<ITimeSource>(),
                                   senseCache,
                                   lightPhysics.CreateTouchPropagationAlgorithm(), lightPhysics.TouchPhysics);
        }


        protected override ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseSource-Active-10":
                    decl.WithTrait(new TouchReceptorTrait<SenseMappingTestContext, ItemReference>(lightPhysics));
                    break;
                case "SenseSource-Active-5":
                    decl.WithTrait(new TouchReceptorTrait<SenseMappingTestContext, ItemReference>(lightPhysics));
                    break;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new TouchReceptorTrait<SenseMappingTestContext, ItemReference>(lightPhysics));
                    break;
            }

            return decl;
        }

        [SetUp]
        public override void SetUp()
        {
            lightPhysics = new TouchPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));
            base.SetUp();

            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorData<TouchSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorState<TouchSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TouchSense, TouchSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TouchSense>>();
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult)]
        public void DoTest(string id, string sourceText, string expectedResultText)
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

            context.ItemEntityRegistry.GetComponent(active10, out SenseSourceState<TouchSense> va).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(active5, out SenseSourceState<TouchSense> vb).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(inactive, out SenseSourceState<TouchSense> vc).Should().BeTrue();

            va.LastPosition.Should().Be(new Position(3, 4, 0, TestMapLayers.One));
            vb.LastPosition.Should().Be(new Position(8, 9, 0, TestMapLayers.One));
            vc.LastPosition.Should().Be(new Position(5, 5, 0, TestMapLayers.One));

            va.State.Should().Be(SenseSourceDirtyState.Active);
            vb.State.Should().Be(SenseSourceDirtyState.Active);
            vc.State.Should().Be(SenseSourceDirtyState.Active);

            va.SenseSource.TryGetValue(out var vaData).Should().BeTrue("because this sense is observed");
            vb.SenseSource.TryGetValue(out var vbData).Should().BeFalse("because we marked this source non-observed.");
            vc.SenseSource.TryGetValue(out var vcData).Should().BeTrue();

            Console.WriteLine("Computed Result:");
            Console.WriteLine(SenseTestHelpers.PrintMap(vaData, vaData.Bounds));
            Console.WriteLine("--");
            var expectedResult = SenseTestHelpers.Parse(expectedResultText);

            // the resulting sense information is stored relative to the sense origin, with the origin point at the centre of the bounds
            // thus the result map must be mapped to the same area.
            SenseTestHelpers.AssertEquals(vaData, expectedResult, expectedResult.GetActiveBounds(), new Position2D(vaData.Radius, vaData.Radius));
        }
    }
}