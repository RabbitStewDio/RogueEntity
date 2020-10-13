using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct LightSourceData
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly float Hue;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly float Saturation;
        [DataMember(Order = 2)]
        [Key(2)]
        public readonly float Intensity;
        [DataMember(Order = 3)]
        [Key(3)]
        public readonly bool Enabled;

        [SerializationConstructor]
        public LightSourceData(float hue, float saturation, float intensity, bool enabled = true)
        {
            Hue = hue;
            Saturation = saturation;
            Intensity = intensity;
            Enabled = enabled;
        }

        public LightSourceData WithColour(float hue, float saturation = 1f)
        {
            return new LightSourceData(hue, saturation, Intensity, Enabled);
        }
        
        public LightSourceData WithIntensity(float intensity)
        {
            return new LightSourceData(Hue, Saturation, intensity, Enabled);
        }
        
        public LightSourceData WithEnabled(bool enabled = true)
        {
            return new LightSourceData(Hue, Saturation, Intensity, Enabled);
        }
    }
}