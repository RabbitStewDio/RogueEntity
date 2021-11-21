using System;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Map;
using RogueEntity.Core.Sensing.Map.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Sensing.Map.Light
{
    public class LightMapSystemTest : SenseMapSystemTestBase<VisionSense, VisionSense, LightSourceDefinition>
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

        const string EmptyRoomGlobalSenseMap = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.101,  0.780,  1.398,  1.938,  2.384,  2.720,  2.929,  3.000,  2.929,  2.720,  2.384
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.780,  1.515,  2.190,  2.789,  3.292,  3.675,  3.917,  4.000,  3.917,  3.675,  3.292
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.566,  1.398,  2.190,  2.929,  3.597,  4.169,  4.615,  4.901,  5.000,  4.901,  4.615,  4.169
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.151,  1.056,  1.938,  2.789,  3.597,  4.343,  5.000,  5.528,  5.877,  6.000,  5.877,  5.528,  5.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.513,  1.456,  2.384,  3.292,  4.169,  5.000,  5.757,  6.394,  6.838,  7.000,  6.838,  6.394,  5.757
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.528,  0.877,  1.000,  0.877,  0.528,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.780,  1.754,  2.720,  3.675,  4.615,  5.528,  6.394,  7.172,  7.764,  8.000,  7.764,  7.172,  6.394
   .   ,   .   ,   .   ,   .   ,   .   ,  0.757,  1.394,  1.838,  2.000,  1.838,  1.394,  0.757,   .   ,   .   ,   .   ,   .   ,   .   ,  0.945,  1.938,  2.929,  3.917,  4.901,  5.877,  6.838,  7.764,  8.586,  9.000,  8.586,  7.764,  6.838
   .   ,   .   ,   .   ,   .   ,  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  9.000, 10.000,  9.000,  8.000,  7.000
   .   ,   .   ,   .   ,   .   ,  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877,   .   ,   .   ,   .   ,   .   ,  0.945,  1.938,  2.929,  3.917,  4.901,  5.877,  6.838,  7.764,  8.586,  9.000,  8.586,  7.764,  6.838
   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   ,   .   ,   .   ,   .   ,  0.780,  1.754,  2.720,  3.675,  4.615,  5.528,  6.394,  7.172,  7.764,  8.000,  7.764,  7.172,  6.394
   .   ,   .   ,   .   ,   .   ,  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877,   .   ,   .   ,   .   ,   .   ,  0.513,  1.456,  2.384,  3.292,  4.169,  5.000,  5.757,  6.394,  6.838,  7.000,  6.838,  6.394,  5.757
   .   ,   .   ,   .   ,   .   ,  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528,   .   ,   .   ,   .   ,   .   ,  0.151,  1.056,  1.938,  2.789,  3.597,  4.343,  5.000,  5.528,  5.877,  6.000,  5.877,  5.528,  5.000
   .   ,   .   ,   .   ,   .   ,   .   ,  0.757,  1.394,  1.838,  2.000,  1.838,  1.394,  0.757,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.566,  1.398,  2.190,  2.929,  3.597,  4.169,  4.615,  4.901,  5.000,  4.901,  4.615,  4.169
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.528,  0.877,  1.000,  0.877,  0.528,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.780,  1.515,  2.190,  2.789,  3.292,  3.675,  3.917,  4.000,  3.917,  3.675,  3.292
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.101,  0.780,  1.398,  1.938,  2.384,  2.720,  2.929,  3.000,  2.929,  2.720,  2.384
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.566,  1.056,  1.456,  1.754,  1.938,  2.000,  1.938,  1.754,  1.456
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.151,  0.513,  0.780,  0.945,  1.000,  0.945,  0.780,  0.513
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        readonly LightPhysicsConfiguration sourcePhysics;

        public LightMapSystemTest()
        {
            this.sourcePhysics = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomGlobalSenseMap)]
        public void Do(string id, string sourceText, string expectedGlobalSenseMap)
        {
            base.PerformTest(id, sourceText, expectedGlobalSenseMap);
        }

        protected override SensoryResistance<VisionSense> Convert(float f)
        {
            return new SensoryResistance<VisionSense>(Percentage.Of(f));
        }

        protected override IReferenceItemDeclaration<ItemReference> AttachTrait(IReferenceItemDeclaration<ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseSource-Active-10":
                    decl.WithTrait(new LightSourceTrait<ItemReference>(sourcePhysics, 10));
                    return decl;
                case "SenseSource-Active-5":
                    decl.WithTrait(new LightSourceTrait<ItemReference>(sourcePhysics, 5));
                    return decl;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new LightSourceTrait<ItemReference>(sourcePhysics, 5, false));
                    return decl;
                default:
                    throw new ArgumentException();
            }
        }

        protected override SenseMappingSystemBase<VisionSense, VisionSense, LightSourceDefinition> CreateSystem()
        {
            return new LightMapSystem(timeSource.AsLazy<ITimeSource>(), new DefaultSenseMapDataBlitter()
            );
        }
    }
}