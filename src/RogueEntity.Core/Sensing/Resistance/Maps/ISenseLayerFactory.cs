namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISenseLayerFactory<TGameContext, TSense>
    {
        void Start(TGameContext context, ISensePropertiesSystem<TGameContext, TSense> system);
        void PrepareLayers(TGameContext ctx, ISensePropertiesSystem<TGameContext, TSense> system);
        void Stop(TGameContext context, ISensePropertiesSystem<TGameContext, TSense> system);
    }
}