namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public interface INoiseSource
    {
        bool TryGetNoiseData(int z, out INoiseView brightnessMap);
    }

    public interface INoiseView
    {
        
    }
}