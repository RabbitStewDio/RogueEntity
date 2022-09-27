namespace RogueEntity.Api.Utils
{
    public interface IGenericLifter<in TBaseType>
    {
        void Invoke<TContext>(TContext contextObject)
            where TContext : TBaseType;
    }

    public interface IGenericLifter<in TBaseTypeA, in TBaseTypeB>
    {
        void Invoke<TContextA, TContextB>()
            where TContextA : TBaseTypeA
            where TContextB : TBaseTypeB;
    }

    public interface IGenericLifterFunction<in TBaseTypeA, in TBaseTypeB>
    {
        object? Invoke<TContextA, TContextB>()
            where TContextA : TBaseTypeA
            where TContextB : TBaseTypeB;
    }
}