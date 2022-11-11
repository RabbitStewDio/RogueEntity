using EnTTSharp;

namespace RogueEntity.Api.Utils
{
    public interface IGenericLifter<in TBaseType>
    {
        void Invoke<TContext>()
            where TContext : notnull, TBaseType;
    }

    public interface IGenericLifterFunction<in TBaseTypeA>
    {
        Optional<TResult> Invoke<TContextA, TResult>()
            where TContextA : TBaseTypeA;
    }
}