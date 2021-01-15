using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public class LightSourceTrait< TItemId> : SenseSourceTraitBase< TItemId, VisionSense, LightSourceDefinition>
        where TItemId : IEntityKey
    {
        readonly ILightPhysicsConfiguration lightPhysics;
        public float Hue { get; }
        public float Saturation { get; }
        public float Intensity { get; }
        public bool Enabled { get; }

        public LightSourceTrait([NotNull] ILightPhysicsConfiguration lightPhysics, float hue, float saturation, float intensity, bool enabled)
        {
            this.lightPhysics = lightPhysics ?? throw new ArgumentNullException(nameof(lightPhysics));
            Hue = hue.Clamp(0, 1);
            Saturation = saturation;
            Intensity = intensity;
            Enabled = enabled;
        }

        /// <summary>
        ///   Creates a white light.
        /// </summary>
        public LightSourceTrait(ILightPhysicsConfiguration lightPhysics,
                                float intensity,
                                bool enabled = true) : this(lightPhysics, 0, 0, intensity, enabled)
        {
        }

        public override ItemTraitId Id => "Core.Common.LightSource";
        public override int Priority => 100;

        protected override bool TryGetInitialValue(out LightSourceDefinition senseDefinition)
        {
            senseDefinition = new LightSourceDefinition(new SenseSourceDefinition(lightPhysics.LightPhysics.DistanceMeasurement,
                                                                                  lightPhysics.LightPhysics.AdjacencyRule,
                                                                                  Intensity), Hue, Saturation, Enabled);
            return true;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return SenseSourceModules.GetSourceRole<VisionSense>().Instantiate<TItemId>();
        }
    }
}