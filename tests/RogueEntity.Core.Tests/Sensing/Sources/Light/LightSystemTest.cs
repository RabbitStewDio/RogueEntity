using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Sensing.Sources.Light
{
    [TestFixture]
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public class LightSystemTest : SenseSystemTestBase<VisionSense, LightSourceDefinition>
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
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  8.000,  8.000,  8.000,  8.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  8.000,  9.000,  9.000,  9.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  8.000,  9.000, 10.000,  9.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  8.000,  9.000,  9.000,  9.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  8.000,  8.000,  8.000,  8.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  5.000,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        LightPhysicsConfiguration physics;

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics()
        {
            return (physics.CreateLightPropagationAlgorithm(), physics.LightPhysics);
        }

        protected override IReferenceItemDeclaration<ItemReference> AttachTrait(IReferenceItemDeclaration<ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseSource-Active-10":
                    decl.WithTrait(new LightSourceTrait<ItemReference>(physics, 10));
                    break;
                case "SenseSource-Active-5":
                    decl.WithTrait(new LightSourceTrait<ItemReference>(physics, 5));
                    break;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new LightSourceTrait<ItemReference>(physics, 5, false));
                    break;
            }

            return decl;
        }

        [SetUp]
        public override void SetUp()
        {
            physics = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));
            base.SetUp();
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult)]
        public void Do(string id, string sourceText, string expectedResultText)
        {
            base.PerformTest(id, sourceText, expectedResultText);
        }
    }
}