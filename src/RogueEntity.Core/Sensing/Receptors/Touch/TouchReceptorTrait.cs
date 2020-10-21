using EnTTSharp.Entities;
using JetBrains.Annotations;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public class TouchReceptorTrait<TGameContext, TActorId>: SenseReceptorTraitBase<TGameContext, TActorId, TouchSense, TouchSense>
        where TActorId : IEntityKey
    {
        public TouchReceptorTrait([NotNull] ITouchPhysicsConfiguration touchPhysics, float intensity = 1) : base(touchPhysics.TouchPhysics, intensity)
        {
        }

        public override string Id => "Core.Sense.Receptor.Touch";
        public override int Priority => 100;
    }
}