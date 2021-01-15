using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Noise;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Noise
{
    public class NoiseSystemTest : SenseSystemTestBase<NoiseSense, NoiseSourceDefinition>
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
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,  8.000,  8.000,  8.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,  9.000,  9.000,  9.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,  9.000, 10.000,  9.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,  9.000,  9.000,  9.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,  8.000,  8.000,  8.000,  8.000,  7.000,  6.000,  5.000,  4.000,  3.000,  2.000,  1.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  6.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        NoisePhysicsConfiguration physics;

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics()
        {
            return (physics.CreateNoisePropagationAlgorithm(), physics.NoisePhysics);
        }

        protected override ReferenceItemDeclaration<ItemReference> AttachTrait(ReferenceItemDeclaration<ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseSource-Active-10":
                    decl.WithTrait(new NoiseSourceTrait<ItemReference>(physics));
                    break;
                case "SenseSource-Active-5":
                    decl.WithTrait(new NoiseSourceTrait<ItemReference>(physics));
                    break;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new NoiseSourceTrait<ItemReference>(physics));
                    break;
            }

            return decl;
        }

        [SetUp]
        public override void SetUp()
        {
            physics = new NoisePhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));
            base.SetUp();
        }

        protected override void PrepareItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            base.PrepareItems(active10, active5, inactive);
            context.ItemResolver.TryUpdateData(active10, new NoiseClip(10, "NoiseA"), out _);
            context.ItemResolver.TryUpdateData(active5, new NoiseClip(5, "NoiseB"), out _);
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult)]
        public void Do(string id, string sourceText, string expectedResultText)
        {
            base.PerformTest(id, sourceText, expectedResultText);
        }
    }
}