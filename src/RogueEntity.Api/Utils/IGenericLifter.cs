namespace RogueEntity.Api.Utils
{
    public interface IGenericLifter<TBaseType>
    {
        void Invoke<TContext>(TContext contextObject)
            where TContext : TBaseType;
    }

    public interface IGenericLifter<TBaseTypeA, TBaseTypeB>
    {
        void Invoke<TContextA, TContextB>()
            where TContextA : TBaseTypeA
            where TContextB : TBaseTypeB;
    }

    public interface IGenericLifterFunction<TBaseTypeA, TBaseTypeB>
    {
        object Invoke<TContextA, TContextB>()
            where TContextA : TBaseTypeA
            where TContextB : TBaseTypeB;
    }
}