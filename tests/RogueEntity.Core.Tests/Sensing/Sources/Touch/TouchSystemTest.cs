using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Touch;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Sensing.Sources.Touch
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public class TouchSystemTest : SenseSystemTestBase<TouchSense, TouchSourceDefinition>
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
   1.000,  1.000,  1.000   
   1.000,  1.000,  1.000   
   1.000,  1.000,  1.000   
";

        TouchSenseReceptorPhysicsConfiguration physics;

        protected override IReferenceItemDeclaration<ItemReference> AttachTrait(IReferenceItemDeclaration<ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseSource-Active-10":
                    decl.WithTrait(new TouchSenseTrait<ItemReference>(physics));
                    break;
                case "SenseSource-Active-5":
                    decl.WithTrait(new TouchSenseTrait<ItemReference>(physics));
                    break;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new TouchSenseTrait<ItemReference>(physics));
                    break;
            }

            return decl;
        }

        [SetUp]
        public override void SetUp()
        {
            physics = new TouchSenseReceptorPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev), new FloodFillWorkingDataSource());
            base.SetUp();

            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorData<TouchSense, TouchSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SensoryReceptorState<TouchSense, TouchSense>>();
            context.ItemEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TouchSense, TouchSense>>();
            context.ItemEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TouchSense, TouchSense>>();
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics()
        {
            return (physics.CreateTouchSensorPropagationAlgorithm(), physics.TouchPhysics);
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult)]
        public void DoTest(string id, string sourceText, string expectedResultText)
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

            context.ItemEntityRegistry.GetComponent(active10, out SenseSourceState<TouchSense> va).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(active5, out SenseSourceState<TouchSense> vb).Should().BeTrue();
            context.ItemEntityRegistry.GetComponent(inactive, out SenseSourceState<TouchSense> vc).Should().BeTrue();

            va.LastPosition.Should().Be(Position.Of(TestMapLayers.One,3, 4));
            vb.LastPosition.Should().Be(Position.Of(TestMapLayers.One,8, 9));
            vc.LastPosition.Should().Be(Position.Of(TestMapLayers.One,5, 5));

            va.State.Should().Be(SenseSourceDirtyState.Active);
            vb.State.Should().Be(SenseSourceDirtyState.Dirty);
            vc.State.Should().Be(SenseSourceDirtyState.Active);

            va.SenseSource.TryGetValue(out var vaData).Should().BeTrue("because this sense is observed");
            vb.SenseSource.TryGetValue(out _).Should().BeFalse("because we marked this source non-observed.");
            vc.SenseSource.TryGetValue(out _).Should().BeTrue();

            Console.WriteLine("Computed Result:");
            Console.WriteLine(TestHelpers.PrintMap(vaData, vaData.Bounds));
            Console.WriteLine("--");
            var expectedResult = SenseTestHelpers.ParseMap(expectedResultText);

            // the resulting sense information is stored relative to the sense origin, with the origin point at the centre of the bounds
            // thus the result map must be mapped to the same area.
            TestHelpers.AssertEquals(vaData, expectedResult, expectedResult.GetActiveBounds(), new GridPosition2D(vaData.Radius, vaData.Radius));
        }
    }
}
