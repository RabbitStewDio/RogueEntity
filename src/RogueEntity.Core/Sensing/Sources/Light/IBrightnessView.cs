namespace RogueEntity.Core.Sensing.Sources.Light
{
    public interface IBrightnessView
    {
        public float this[int x, int y] { get; }
    }
}