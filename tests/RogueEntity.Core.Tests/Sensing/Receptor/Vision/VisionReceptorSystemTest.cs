using System;
using EnTTSharp.Entities.Systems;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Vision
{
    public class VisionReceptorSystemTest : SenseReceptorSystemBase<VisionSense, VisionSense, LightSourceDefinition>
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

        const string EmptyRoomResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  8.000,  8.000,  8.000,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  9.000,  9.000,  9.000,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  9.000, 10.000,  9.000,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  9.000,  9.000,  9.000,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  8.000,  8.000,  8.000,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  1.000,  2.000,  3.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        readonly LightPhysicsConfiguration physics;

        public VisionReceptorSystemTest()
        {
            this.physics = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));
        }

        protected override SensoryResistance Convert(float f)
        {
            return new SensoryResistance(Percentage.Of(f), Percentage.Empty, Percentage.Empty, Percentage.Empty);
        }

        protected override Action<SenseMappingTestContext> CreateCopyAction()
        {
            var builder = context.ItemEntityRegistry.BuildSystem()
                                 .WithContext<SenseMappingTestContext>();

            var omniSystem = new OmnidirectionalSenseReceptorSystem<VisionSense, VisionSense>(senseSystem, new DefaultSenseDataBlitter());
            return builder.CreateSystem<SingleLevelSenseDirectionMapData<VisionSense, VisionSense>, SensoryReceptorState<VisionSense>>(omniSystem.CopySenseSourcesToVisionField);
        }

        protected override ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseReceptor-Active-10":
                    decl.WithTrait(new VisionSenseTrait<SenseMappingTestContext, ItemReference>(physics, 10));
                    return decl;
                case "SenseReceptor-Active-5":
                    decl.WithTrait(new VisionSenseTrait<SenseMappingTestContext, ItemReference>(physics, 5));
                    return decl;
                case "SenseReceptor-Inactive-5":
                    decl.WithTrait(new VisionSenseTrait<SenseMappingTestContext, ItemReference>(physics, 5, false));
                    return decl;
                case "SenseSource-Active-10":
                    decl.WithTrait(new LightSourceTrait<SenseMappingTestContext, ItemReference>(physics, 10));
                    return decl;
                case "SenseSource-Active-5":
                    decl.WithTrait(new LightSourceTrait<SenseMappingTestContext, ItemReference>(physics, 5));
                    return decl;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new LightSourceTrait<SenseMappingTestContext, ItemReference>(physics, 5, false));
                    return decl;
                default:
                    throw new ArgumentException();
            }
        }

        protected override SenseReceptorSystem<VisionSense, VisionSense> CreateSystem()
        {
            return new VisionReceptorSystem(senseProperties.AsLazy<ISensePropertiesSource>(),
                                            senseCache.AsLazy<ISenseStateCacheProvider>(),
                                            senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                            timeSource.AsLazy<ITimeSource>(),
                                            physics.LightPhysics,
                                            physics.CreateLightPropagationAlgorithm());
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult)]
        public void Do(string id, string sourceText, string expectedResultText)
        {
            base.PerformTest(id, sourceText, expectedResultText);
        }
    }
}