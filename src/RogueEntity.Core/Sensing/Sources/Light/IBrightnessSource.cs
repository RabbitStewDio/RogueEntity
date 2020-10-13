namespace RogueEntity.Core.Sensing.Sources.Light
{
    public interface IBrightnessSource
    {
        bool TryGetLightData(int z, out IBrightnessView brightnessMap);
    }
}