namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISenseLayerFactory<TGameContext>
    {
        void Start(TGameContext context, ISensePropertiesSystem<TGameContext> system);
        void PrepareLayers(TGameContext ctx, ISensePropertiesSystem<TGameContext> system);
        void Stop(TGameContext context, ISensePropertiesSystem<TGameContext> system);
    }
}