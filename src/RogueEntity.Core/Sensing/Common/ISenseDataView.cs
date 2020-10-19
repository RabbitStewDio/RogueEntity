namespace RogueEntity.Core.Sensing.Common
{
    public interface ISenseDataView
    {
        float QueryBrightness(int x, int y);
        SenseDirectionStore QueryDirection(int x, int y);
        
        bool TryQuery(int x,
                      int y,
                      out float intensity,
                      out SenseDirectionStore directionality);
    }
}