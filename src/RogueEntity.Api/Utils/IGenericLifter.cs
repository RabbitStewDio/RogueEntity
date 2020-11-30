namespace RogueEntity.Api.Utils
{
    public interface IGenericLifter<TBaseType>
    {
        void Invoke<TContext>(TContext contextObject)
            where TContext : TBaseType;
    }
}